using System.Security.Cryptography;
using System.Text;
using illusion.Common.Constants;
using illusion.Common.Utils;

namespace illusion.Memory;

/// <summary>
/// Local-only encrypted User Private Plane file store. The store never performs
/// network access and writes each record atomically to a path derived from its
/// namespace/key pair.
/// </summary>
public sealed class UserPrivatePlaneStore
{
    private static readonly byte[] Magic = "IAUPP1"u8.ToArray();
    private readonly string _rootDirectory;
    private readonly bool _encryptionEnabled;

    public UserPrivatePlaneStore(string rootDirectory, bool encryptionEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("root directory cannot be empty", nameof(rootDirectory));

        _rootDirectory = Path.GetFullPath(rootDirectory);
        _encryptionEnabled = encryptionEnabled;
        Directory.CreateDirectory(_rootDirectory);
    }

    public void Put<T>(string ns, string key, T value, string passphrase)
    {
        ValidateAddress(ns, key);
        ArgumentNullException.ThrowIfNull(value);
        if (_encryptionEnabled && string.IsNullOrWhiteSpace(passphrase))
            throw new ArgumentException("passphrase is required when encryption is enabled", nameof(passphrase));

        var payload = SerializationUtils.Serialize(value);
        var associatedData = Encoding.UTF8.GetBytes($"{ns}\n{key}");
        byte[] bytes;

        if (_encryptionEnabled)
        {
            var salt = RandomNumberGenerator.GetBytes(CryptoUtils.Pbkdf2SaltSize);
            var derivedKey = CryptoUtils.DeriveAes256Key(passphrase, salt);
            var encrypted = CryptoUtils.EncryptAes256Gcm(payload, derivedKey, associatedData);
            bytes = Combine(Magic, new byte[] { 1 }, salt, encrypted);
        }
        else
        {
            bytes = Combine(Magic, new byte[] { 0 }, payload);
        }

        var path = ResolvePath(ns, key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        File.WriteAllBytes(temp, bytes);
        File.Move(temp, path, overwrite: true);
    }

    public T Get<T>(string ns, string key, string passphrase)
    {
        ValidateAddress(ns, key);
        var path = ResolvePath(ns, key);
        if (!File.Exists(path))
            throw new FileNotFoundException($"UPP record not found for {ns}{key}", path);

        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < Magic.Length + 1 || !bytes.AsSpan(0, Magic.Length).SequenceEqual(Magic))
            throw new InvalidDataException("invalid UPP record header");

        var encrypted = bytes[Magic.Length] == 1;
        var offset = Magic.Length + 1;
        byte[] payload;
        if (encrypted)
        {
            if (string.IsNullOrWhiteSpace(passphrase))
                throw new ArgumentException("passphrase is required for encrypted UPP records", nameof(passphrase));
            if (bytes.Length < offset + CryptoUtils.Pbkdf2SaltSize)
                throw new InvalidDataException("invalid encrypted UPP record");

            var salt = bytes.AsSpan(offset, CryptoUtils.Pbkdf2SaltSize).ToArray();
            offset += CryptoUtils.Pbkdf2SaltSize;
            var encryptedPayload = bytes.AsSpan(offset).ToArray();
            var derivedKey = CryptoUtils.DeriveAes256Key(passphrase, salt);
            var associatedData = Encoding.UTF8.GetBytes($"{ns}\n{key}");
            payload = CryptoUtils.DecryptAes256Gcm(encryptedPayload, derivedKey, associatedData);
        }
        else
        {
            payload = bytes.AsSpan(offset).ToArray();
        }

        return SerializationUtils.Deserialize<T>(payload);
    }

    public bool Exists(string ns, string key)
    {
        ValidateAddress(ns, key);
        return File.Exists(ResolvePath(ns, key));
    }

    private string ResolvePath(string ns, string key)
    {
        var fileName = Base64UrlEncode(Encoding.UTF8.GetBytes($"{ns}\n{key}")) + UPPConstants.StorageFileExtension;
        return Path.Combine(_rootDirectory, fileName[..2], fileName);
    }

    private static void ValidateAddress(string ns, string key)
    {
        if (string.IsNullOrWhiteSpace(ns) || !ns.StartsWith(UPPConstants.NamespacePrefix, StringComparison.Ordinal))
            throw new ArgumentException($"namespace must start with {UPPConstants.NamespacePrefix}", nameof(ns));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("key cannot be empty", nameof(key));
    }

    private static byte[] Combine(params byte[][] parts)
    {
        var output = new byte[parts.Sum(p => p.Length)];
        var offset = 0;
        foreach (var part in parts)
        {
            Buffer.BlockCopy(part, 0, output, offset, part.Length);
            offset += part.Length;
        }
        return output;
    }

    private static string Base64UrlEncode(byte[] data) => Convert.ToBase64String(data)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');
}

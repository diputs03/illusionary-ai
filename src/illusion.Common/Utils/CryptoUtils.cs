using System.Security.Cryptography;
using System.Text;

namespace illusion.Common.Utils;

/// <summary>
/// Cryptographic primitives used by the trusted local runtime. Methods validate
/// inputs and expose explicit authenticated-encryption semantics.
/// </summary>
public static class CryptoUtils
{
    public const int Aes256KeySize = 32;
    public const int AesGcmNonceSize = 12;
    public const int AesGcmTagSize = 16;
    public const int Pbkdf2SaltSize = 16;
    public const int Pbkdf2Iterations = 210_000;

    public static byte[] ComputeSha256(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return SHA256.HashData(data);
    }

    public static byte[] DeriveAes256Key(string passphrase, byte[] salt)
    {
        if (string.IsNullOrWhiteSpace(passphrase))
            throw new ArgumentException("passphrase cannot be empty", nameof(passphrase));
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length < Pbkdf2SaltSize)
            throw new ArgumentException($"salt must be at least {Pbkdf2SaltSize} bytes", nameof(salt));

        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(passphrase),
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            Aes256KeySize);
    }

    public static byte[] EncryptAes256Gcm(byte[] plaintext, byte[] key, byte[]? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        ValidateAes256Key(key);

        var nonce = RandomNumberGenerator.GetBytes(AesGcmNonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcmTagSize];

        using var aes = new AesGcm(key, AesGcmTagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

        var result = new byte[AesGcmNonceSize + AesGcmTagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);
        return result;
    }

    public static byte[] DecryptAes256Gcm(byte[] encrypted, byte[] key, byte[]? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(encrypted);
        ValidateAes256Key(key);
        if (encrypted.Length < AesGcmNonceSize + AesGcmTagSize)
            throw new CryptographicException("encrypted payload is too short");

        var nonce = encrypted.AsSpan(0, AesGcmNonceSize).ToArray();
        var tag = encrypted.AsSpan(AesGcmNonceSize, AesGcmTagSize).ToArray();
        var ciphertext = encrypted.AsSpan(AesGcmNonceSize + AesGcmTagSize).ToArray();
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, AesGcmTagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);
        return plaintext;
    }

    public static bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);

        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static byte[] Sign(byte[] data, byte[] privateKey)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(privateKey);

        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKey, out _);
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static void ValidateAes256Key(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != Aes256KeySize)
            throw new ArgumentException($"AES-256 key must be exactly {Aes256KeySize} bytes", nameof(key));
    }
}

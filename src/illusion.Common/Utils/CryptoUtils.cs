namespace illusion.Common.Utils;

/// <summary>
/// encrypt utils
/// </summary>
public static class CryptoUtils
{
    private const int Sha256HashSize = 32;

    public static byte[] ComputeSha256(byte[] data)
    {
        // Note that the sha256 instance will be automatically managing null input
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return sha256.ComputeHash(data);
    }
    public static bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
    {
        using var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
        return rsa.VerifyData(data, signature,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1
            );
    }
}
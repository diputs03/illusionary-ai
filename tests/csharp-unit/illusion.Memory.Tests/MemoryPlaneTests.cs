using System.Security.Cryptography;
using illusion.Memory;
using illusion.Common.Utils;
using Xunit;

namespace illusion.Memory.Tests;

public sealed class MemoryPlaneTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "illusion-upp-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void UserPrivatePlaneStore_RoundTripsEncryptedPayload()
    {
        var store = new UserPrivatePlaneStore(_tempDirectory);
        const string ns = "upp://default/";
        const string key = "secret-note";

        store.Put(ns, key, "private value", "correct horse battery staple");

        Assert.True(store.Exists(ns, key));
        Assert.Equal("private value", store.Get<string>(ns, key, "correct horse battery staple"));
        Assert.ThrowsAny<CryptographicException>(() => store.Get<string>(ns, key, "wrong passphrase"));
    }

    [Fact]
    public void GroundTruthPlaneVerifier_AcceptsOnlySignedModulePayload()
    {
        using var rsa = RSA.Create(2048);
        var publicKey = rsa.ExportSubjectPublicKeyInfo();
        var privateKey = rsa.ExportPkcs8PrivateKey();
        var unsigned = new GroundTruthModule(
            "ggtp://core/logic",
            "core-logic",
            "1.0.0",
            new byte[] { 1, 2, 3 },
            Array.Empty<byte>(),
            publicKey);
        var signed = unsigned with { Signature = CryptoUtils.Sign(unsigned.SigningPayload(), privateKey) };
        var tampered = signed with { Payload = new byte[] { 9, 9, 9 } };
        var verifier = new GroundTruthPlaneVerifier();

        Assert.True(verifier.Verify(signed));
        Assert.False(verifier.Verify(tampered));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }
}

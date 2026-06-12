using illusion.Common.Utils;

namespace illusion.Memory;

/// <summary>
/// Verifies GGTP modules without accepting user/private data. Failed signatures
/// are represented as false rather than implicit fallback trust.
/// </summary>
public sealed class GroundTruthPlaneVerifier
{
    public bool Verify(GroundTruthModule module)
    {
        ArgumentNullException.ThrowIfNull(module);
        module.Validate();
        return CryptoUtils.VerifySignature(module.SigningPayload(), module.Signature, module.PublicKey);
    }
}

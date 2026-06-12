using illusion.Common.Constants;
using illusion.Common.Utils;

namespace illusion.Memory;

/// <summary>
/// Signed, read-only global ground-truth module descriptor.
/// </summary>
public sealed record GroundTruthModule(
    string Namespace,
    string Name,
    string Version,
    byte[] Payload,
    byte[] Signature,
    byte[] PublicKey)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Namespace) || !Namespace.StartsWith(GGTPConstants.NamespacePrefix, StringComparison.Ordinal))
            throw new ArgumentException($"module namespace must start with {GGTPConstants.NamespacePrefix}", nameof(Namespace));
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("module name cannot be empty", nameof(Name));
        if (string.IsNullOrWhiteSpace(Version))
            throw new ArgumentException("module version cannot be empty", nameof(Version));
        ArgumentNullException.ThrowIfNull(Payload);
        ArgumentNullException.ThrowIfNull(Signature);
        ArgumentNullException.ThrowIfNull(PublicKey);
    }

    public byte[] SigningPayload() => SerializationUtils.Serialize(new GroundTruthModulePayload(Namespace, Name, Version, Payload));
}

public sealed record GroundTruthModulePayload(string Namespace, string Name, string Version, byte[] Payload);

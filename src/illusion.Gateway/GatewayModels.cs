namespace illusion.Gateway;

public sealed record GatewayInfo(string SystemName, string LogLevel, bool UppEncryptionEnabled, string UppStorageDirectory);
public sealed record ProofRequest(string Predicate, string ObjectId, string ObjectName);
public sealed record ProofResponse(bool IsSuccess, string TraceId, IReadOnlyList<string> Steps, string? ErrorMessage);
public sealed record GenerationResponse(string Code);

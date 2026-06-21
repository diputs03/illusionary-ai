using illusion.Common.Utils;
using illusion.CoreLogic.Prover;
using illusion.Generator;
using illusion.Memory;

namespace illusion.Gateway;

/// <summary>
/// Application service boundary used by CLI/API hosts. It coordinates parsing,
/// proof, generation, and memory without embedding host-specific concerns.
/// </summary>
public sealed class IllusionGatewayService
{
    public GatewayInfo GetInfo() => new(
        ConfigLoader.GetSystemName(),
        ConfigLoader.GetLogLevel(),
        ConfigLoader.IsUPPEncryptionEnabled(),
        ConfigLoader.GetUPPStorageDirectory());

    public ProofResponse ProveDirect(ProofRequest request)
    {
        var proposition = ToExpression(request);
        var trace = new AnalyticalProver(new[] { proposition }).Prove(proposition);
        return new ProofResponse(
            trace.IsSuccess,
            trace.TraceId,
            trace.Steps.Select(s => $"{s.StepNumber}. {s.RuleName}: {s.StepExpression.ToCanonicalString()}").ToList().AsReadOnly(),
            trace.ErrorMessage);
    }

    public GenerationResponse GenerateDirect(ProofRequest request)
    {
        var proposition = ToExpression(request);
        var generator = new DeterministicConstructiveProver(new AnalyticalProver(new[] { proposition }));
        return new GenerationResponse(generator.GenerateCode(proposition));
    }

    public void PutPrivateRecord(string ns, string key, string value, string passphrase)
    {
        var store = CreateStore();
        store.Put(ns, key, value, passphrase);
    }

    public string GetPrivateRecord(string ns, string key, string passphrase)
    {
        var store = CreateStore();
        return store.Get<string>(ns, key, passphrase);
    }

    private static UserPrivatePlaneStore CreateStore() =>
        new(ConfigLoader.GetUPPStorageDirectory(), ConfigLoader.IsUPPEncryptionEnabled());

    private static Common.Types.Expression ToExpression(ProofRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new Common.Types.Expression(request.Predicate, new Common.Types.Object(request.ObjectId, request.ObjectName));
    }
}

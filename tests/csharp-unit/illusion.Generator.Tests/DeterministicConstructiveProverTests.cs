using illusion.CoreLogic.Prover;

namespace illusion.Generator.Tests;

public class DeterministicConstructiveProverTests
{
    [Fact]
    public void GenerateCode_RefusesToGenerateWhenPropositionIsNotProvable()
    {
        var prover = new DeterministicConstructiveProver(new AnalyticalProver(Array.Empty<Common.Types.Expression>()));

        Assert.Throws<Common.Types.IAException<InvalidOperationException>>(() =>
            prover.GenerateCode(Expr("Unknown", "obj", "Object")));
    }

    [Fact]
    public void GenerateCode_GeneratesTraceableCSharpForProof()
    {
        var proposition = Expr("Safe", "module-a", "ModuleA");
        var prover = new DeterministicConstructiveProver(new AnalyticalProver(new[] { proposition }));

        var code = prover.GenerateCode(proposition);

        Assert.Contains("GeneratedProofArtifact", code);
        Assert.Contains(proposition.ToCanonicalString(), code);
    }

    private static Common.Types.Expression Expr(string predicate, string objectId, string objectName) =>
        new(predicate, new Common.Types.Object(objectId, objectName));
}

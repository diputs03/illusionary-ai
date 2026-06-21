using illusion.CoreLogic.Prover;
using Xunit;

namespace illusion.CoreLogic.Tests;

public class AnalyticalProverTests
{
    [Fact]
    public void Prove_WithDirectAxiom_ReturnsSuccessfulTrace()
    {
        var proposition = Expr("Safe", "module-a", "ModuleA");
        var prover = new AnalyticalProver(new[] { proposition });

        var trace = prover.Prove(proposition);

        Assert.True(trace.IsSuccess);
        Assert.Contains(trace.Steps, step => step.RuleName == "axiom" && step.StepExpression.Equals(proposition));
    }

    [Fact]
    public void Prove_WithHornRule_DerivesConclusionWithPremiseTrace()
    {
        var verified = Expr("Verified", "module-a", "ModuleA");
        var safeTemplate = Expr("Safe", "?id", "?name");
        var verifiedTemplate = Expr("Verified", "?id", "?name");
        var rule = new FormalRule("verified-implies-safe", new[] { verifiedTemplate }, safeTemplate);
        var prover = new AnalyticalProver(new[] { verified }, new[] { rule });

        var trace = prover.Prove(Expr("Safe", "module-a", "ModuleA"));

        Assert.True(trace.IsSuccess);
        var derived = Assert.Single(trace.Steps, step => step.RuleName == "verified-implies-safe");
        Assert.NotEmpty(derived.PremiseStepNumbers);
    }

    private static Common.Types.Expression Expr(string predicate, string objectId, string objectName) =>
        new(predicate, new Common.Types.Object(objectId, objectName));
}

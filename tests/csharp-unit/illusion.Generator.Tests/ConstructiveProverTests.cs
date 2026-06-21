using illusion.CoreLogic.Prover;
using illusion.Generator;
using Xunit;

namespace illusion.Generator.Tests;

public class ConstructiveProverTests
{
    [Fact]
    public void GenerateCode_RejectsUnsupportedLanguage()
    {
        var proposition = new Common.Types.Expression("Safe", new Common.Types.Object("module-a", "ModuleA"));
        var generator = new DeterministicConstructiveProver(new AnalyticalProver(new[] { proposition }));

        Assert.Throws<Common.Types.IAException<NotSupportedException>>(() => generator.GenerateCode(proposition, "Python"));
    }
}

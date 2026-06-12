using illusion.Common.Types;
using Xunit;

namespace illusion.Common.Tests;

public class ReasoningTypeTests
{
    [Fact]
    public void Expression_UsesValueEqualityAndCanonicalString()
    {
        var left = new Types.Expression("IsValid", new Types.Object("obj-001", "TestObject"));
        var right = new Types.Expression("IsValid", new Types.Object("obj-001", "TestObject"));

        Assert.Equal(left, right);
        Assert.Equal("IsValid(obj-001:TestObject)", left.ToCanonicalString());
    }

    [Fact]
    public void Constraint_Verify_ReturnsTrueOnlyForKnownFacts()
    {
        var known = new Types.Expression("Known", new Types.Object("obj-001", "Object"));
        var missing = new Types.Expression("Missing", new Types.Object("obj-001", "Object"));
        var state = new State(new[] { known });

        Assert.True(new Types.Constraint(Types.Constraint.ConstraintType.Precondition, known).Verify(state));
        Assert.False(new Types.Constraint(Types.Constraint.ConstraintType.Precondition, missing).Verify(state));
    }

    [Fact]
    public void State_AddVerifiedConclusion_IsIdempotent()
    {
        var conclusion = new Types.Expression("Verified", new Types.Object("obj-001", "Object"));
        var state = new State(Array.Empty<Types.Expression>());

        state.AddVerifiedConclusion(conclusion);
        state.AddVerifiedConclusion(conclusion);

        Assert.True(state.ContainsFact(conclusion));
        Assert.Single(state.VerifiedConclusions);
    }
}

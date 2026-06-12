using illusion.Common.Types;

namespace illusion.MetaStrategy.Tests;

public class DeterministicPlannerTests
{
    [Fact]
    public void Plan_OnlySchedulesActionsWithSatisfiedConstraints()
    {
        var fact = new Expression("Ready", new Common.Types.Object("task-a", "TaskA"));
        var missing = new Expression("Ready", new Common.Types.Object("task-b", "TaskB"));
        var state = new State(new[] { fact });
        var runnable = new Common.Types.Action("2", "Runnable", Common.Types.Action.ActionType.Plan, new[] { new Constraint(Constraint.ConstraintType.Precondition, fact) });
        var blocked = new Common.Types.Action("1", "Blocked", Common.Types.Action.ActionType.Plan, new[] { new Constraint(Constraint.ConstraintType.Precondition, missing) });

        var plan = new DeterministicPlanner().Plan(state, new[] { blocked, runnable });

        Assert.True(plan.IsComplete);
        var step = Assert.Single(plan.Steps);
        Assert.Equal("Runnable", step.Action.Name);
    }
}

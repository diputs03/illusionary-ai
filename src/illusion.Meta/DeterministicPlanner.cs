using illusion.Common.Types;

namespace illusion.Meta;

/// <summary>
/// Small deterministic planner that selects actions whose constraints are already
/// satisfied by state facts. It is intentionally conservative: no action is
/// scheduled when a precondition cannot be formally verified.
/// </summary>
public sealed class DeterministicPlanner
{
    public StrategyPlan Plan(State state, IEnumerable<illusion.Common.Types.Action> actions)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(actions);

        var steps = new List<PlanStep>();
        foreach (var action in actions.OrderBy(a => a.Id, StringComparer.Ordinal))
        {
            var constraints = action.Constraints.ToList();
            var unsatisfied = constraints.Where(c => !c.Verify(state)).ToList();
            if (unsatisfied.Count > 0)
                continue;

            steps.Add(new PlanStep(steps.Count + 1, action, constraints.AsReadOnly()));
        }

        return steps.Count == 0
            ? new StrategyPlan(steps, isComplete: false, "no actions had formally satisfied constraints")
            : new StrategyPlan(steps, isComplete: true);
    }
}

namespace illusion.Meta;

public sealed class StrategyPlan
{
    public IReadOnlyList<PlanStep> Steps { get; }
    public bool IsComplete { get; }
    public string? FailureReason { get; }

    public StrategyPlan(IEnumerable<PlanStep> steps, bool isComplete, string? failureReason = null)
    {
        Steps = (steps ?? throw new ArgumentNullException(nameof(steps))).ToList().AsReadOnly();
        IsComplete = isComplete;
        FailureReason = failureReason;
    }
}

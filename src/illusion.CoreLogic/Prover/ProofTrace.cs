using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// Complete proof trace for a single proof attempt, including all steps, results,
/// timings, and error metadata.
/// </summary>
public sealed class ProofTrace
{
    public string TraceId { get; }
    public Expression TargetProposition { get; }
    public bool IsSuccess { get; }
    public IReadOnlyList<ProofStep> Steps { get; }
    public long ElapsedMilliseconds { get; }
    public string? ErrorMessage { get; }

    public ProofTrace(string traceId, Expression targetProposition, bool isSuccess,
        IEnumerable<ProofStep> steps, long elapsedMilliseconds, string? errorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(traceId))
            throw new IAException<ArgumentException>("trace id cannot be empty");

        TraceId = traceId.Trim();
        TargetProposition = targetProposition ?? throw new IAException<ArgumentNullException>(nameof(targetProposition));
        IsSuccess = isSuccess;
        Steps = (steps ?? throw new IAException<ArgumentNullException>(nameof(steps))).ToList().AsReadOnly();
        ElapsedMilliseconds = elapsedMilliseconds;
        ErrorMessage = errorMessage;
    }
}

public sealed class ProofStep
{
    public int StepNumber { get; }
    public string RuleName { get; }
    public Expression StepExpression { get; }
    public IReadOnlyList<int> PremiseStepNumbers { get; }
    public string? Description { get; }

    public ProofStep(int stepNumber, string ruleName, Expression stepExpression, string? description = null,
        IEnumerable<int>? premiseStepNumbers = null)
    {
        if (stepNumber <= 0)
            throw new IAException<ArgumentOutOfRangeException>("proof step number must be positive");
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new IAException<ArgumentException>("rule name cannot be empty");

        StepNumber = stepNumber;
        RuleName = ruleName.Trim();
        StepExpression = stepExpression ?? throw new IAException<ArgumentNullException>(nameof(stepExpression));
        PremiseStepNumbers = (premiseStepNumbers ?? Array.Empty<int>()).ToList().AsReadOnly();
        Description = description;
    }
}

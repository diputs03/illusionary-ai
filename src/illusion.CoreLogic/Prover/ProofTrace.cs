using illusion.Common.Types;

namespace illusion.CoreLogic.Prover;

/// <summary>
/// complete proof trace for a single proof attempt, including all steps, results, and metadata.
/// </summary>
public class ProofTrace
{
    public string TraceId { get; }
    public Common.Types.Expression TargetProposition { get; }
    public bool IsSuccess { get; }
    public IReadOnlyList<ProofStep> Steps { get; }
    public long ElapsedMilliseconds { get; }
    public string? ErrorMessage { get; }

    public ProofTrace(string traceId, Common.Types.Expression targetProposition, bool isSuccess,
        IEnumerable<ProofStep> steps, long elapsedMilliseconds, string? errorMessage = null)
    {
        TraceId = traceId;
        TargetProposition = targetProposition;
        IsSuccess = isSuccess;
        Steps = steps.ToList().AsReadOnly();
        ElapsedMilliseconds = elapsedMilliseconds;
        ErrorMessage = errorMessage;
    }
}
public class ProofStep
{
    public int StepNumber { get; }
    public string RuleName { get; }
    public Common.Types.Expression StepExpression { get; }
    public string? Description { get; }

    public ProofStep(int stepNumber, string ruleName, Common.Types.Expression stepExpression, string? description = null)
    {
        StepNumber = stepNumber;
        RuleName = ruleName;
        StepExpression = stepExpression;
        Description = description;
    }
}
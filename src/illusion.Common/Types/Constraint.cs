namespace illusion.Common.Types;

/// <summary>
/// Action constraints must be satisfied for an action to execute (preconditions),
/// hold after execution (postconditions), or terminate (termination conditions).
/// </summary>
public sealed class Constraint
{
    public enum ConstraintType
    {
        Precondition,
        Postcondition,
        Termination
    }

    public ConstraintType Type { get; }
    public Expression ConstraintExpression { get; }
    public string Description { get; }

    public Constraint(ConstraintType type, Expression constraintExpression, string description = "")
    {
        Type = type;
        ConstraintExpression = constraintExpression ?? throw new IAException<ArgumentNullException>(nameof(constraintExpression));
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// Deterministically verifies the constraint against a formal state. A
    /// constraint is satisfied only when its expression has already been accepted
    /// as an axiom or verified conclusion; no heuristic fallback is used.
    /// </summary>
    public bool Verify(State currentState)
    {
        if (currentState is null)
            throw new IAException<ArgumentNullException>(nameof(currentState));

        return currentState.ContainsFact(ConstraintExpression);
    }
}

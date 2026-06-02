using illusion.Common.Types;

namespace illusion.Common.Types;

/// <summary>
/// Action constraints must be satisfied for an action
/// executed (preconditions), hold after execution (postconditions), terminate (termination conditions).
/// </summary>
public class Constraint
{
    public enum ConstraintType
    {
        Precondition,
        Postcondition,
        Termination
    }

    public ConstraintType Type { get; }
    public Types.Expression ConstraintExpression { get; }
    public string Description { get; }

    public Constraint(ConstraintType type, Types.Expression constraintExpression, string description = "")
    {
        Type = type;
        ConstraintExpression = constraintExpression ?? throw new IAException<ArgumentNullException>(nameof(constraintExpression));
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// verify if the constraint is satisfied in the given state
    /// TODO: implement the actual verification logic
    /// </summary>
    public bool Verify(State currentState)
    {
        if (currentState == null)
            throw new IAException<ArgumentNullException>(nameof(currentState));
        // TODO: implement the actual verification logic
        return true;
    }
}
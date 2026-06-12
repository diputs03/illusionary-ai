namespace illusion.Common.Types;

/// <summary>
/// Essential predicate expression E(O). Expressions are immutable value objects so
/// they can be used safely as deterministic facts inside proof state.
/// </summary>
public sealed class Expression : IEquatable<Expression>
{
    public string PredicateName { get; }
    public Object TargetObject { get; }

    public Expression(string predicateName, Object targetObject)
    {
        if (string.IsNullOrWhiteSpace(predicateName))
            throw new IAException<ArgumentException>("predicate name cannot be empty");

        PredicateName = predicateName.Trim();
        TargetObject = targetObject ?? throw new IAException<ArgumentNullException>(nameof(targetObject));
    }

    public bool Evaluate(State? state = null) => state?.ContainsFact(this) ?? true;

    public string ToCanonicalString() => $"{PredicateName}({TargetObject.Id}:{TargetObject.Name})";

    public bool Equals(Expression? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(PredicateName, other.PredicateName)
        && TargetObject.Equals(other.TargetObject);

    public override bool Equals(object? obj) => Equals(obj as Expression);

    public override int GetHashCode() => HashCode.Combine(
        StringComparer.Ordinal.GetHashCode(PredicateName),
        TargetObject);

    public override string ToString() => ToCanonicalString();
}

namespace illusion.Common.Types;

/// <summary>
/// Essential predicate expression E(O).
/// </summary>
public class Expression
{
    public string PredicateName { get; }
    public Object TargetObject { get; }

    public Expression(string predicateName, Object targetObject)
    {
        if (string.IsNullOrWhiteSpace(predicateName))
            throw new ArgumentException("predicate name cannot be empty", nameof(predicateName));

        PredicateName = predicateName;
        TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
    }

    public bool Evaluate()
    {
        // TODO: Implement the actual evaluation logic
        return true;
    }
}
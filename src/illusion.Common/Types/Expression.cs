namespace illusion.Common.Types;

/// <summary>
/// Essential predicate expression E(O).
/// </summary>
public class Expression
{
    public string PredicateName { get; }
    public Entity TargetObject { get; }

    public Expression(string predicateName, Entity targetObject)
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
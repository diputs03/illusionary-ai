using illusion.Common.Types;

namespace illusion.Common.Types;

/// <summary>
/// Essential predicate expression E(O).
/// </summary>
public class Expression
{
    public string PredicateName { get; }
    public Object TargetObject { get; }

    public Expression(string predicateName, Types.Object targetObject)
    {
        if (string.IsNullOrWhiteSpace(predicateName))
            throw new IAException<ArgumentException>("predicate name cannot be empty");

        PredicateName = predicateName;
        TargetObject = targetObject ?? throw new IAException<ArgumentNullException>(nameof(targetObject));
    }

    public bool Evaluate()
    {
        // TODO: Implement the actual evaluation logic
        return true;
    }
}
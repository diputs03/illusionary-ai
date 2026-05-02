namespace illusion.Common.Types;

/// <summary>
/// global state of the system
/// includes all objects, axioms, verified conclusions
/// </summary>
public class State
{
    public enum StatePhase
    {
        Initial,
        Executing,
        Completed,
        Failed
    }

    public StatePhase Phase { get; private set; }
    public IReadOnlyDictionary<string, Object> Objects { get; }
    public IReadOnlyList<Expression> Axioms { get; }
    public IReadOnlyList<Expression> VerifiedConclusions { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public State(IEnumerable<Expression> axioms)
    {
        Phase = StatePhase.Initial;
        Objects = new Dictionary<string, Object>().AsReadOnly();
        Axioms = axioms?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(axioms));
        VerifiedConclusions = new List<Expression>().AsReadOnly();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// add a verified conclusion to the state
    /// </summary>
    public void AddVerifiedConclusion(Expression conclusion)
    {
        if (conclusion == null)
            throw new ArgumentNullException(nameof(conclusion));

        var list = VerifiedConclusions.ToList();
        list.Add(conclusion);
        VerifiedConclusions = list.AsReadOnly();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// update the phase of the state
    /// </summary>
    public void UpdatePhase(StatePhase newPhase)
    {
        Phase = newPhase;
        UpdatedAt = DateTime.UtcNow;
    }
}
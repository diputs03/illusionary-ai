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
    public IReadOnlyDictionary<string, Types.Object> Objects { get; }
    public IReadOnlyList<Types.Expression> Axioms { get; }
    public IReadOnlyList<Types.Expression> VerifiedConclusions { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public State(IEnumerable<Types.Expression> axioms)
    {
        Phase = StatePhase.Initial;
        Objects = new Dictionary<string, Types.Object>().AsReadOnly();
        Axioms = axioms?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(axioms));
        VerifiedConclusions = new List<Types.Expression>().AsReadOnly();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// add a verified conclusion to the state
    /// </summary>
    public void AddVerifiedConclusion(Types.Expression conclusion)
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
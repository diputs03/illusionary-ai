namespace illusion.Common.Types;

/// <summary>
/// Global deterministic state of the system, including objects, axioms, and
/// verified conclusions.
/// </summary>
public sealed class State
{
    private readonly Dictionary<string, Object> _objects;
    private readonly HashSet<Expression> _facts;
    private readonly List<Expression> _verifiedConclusions;

    public enum StatePhase
    {
        Initial,
        Executing,
        Completed,
        Failed
    }

    public StatePhase Phase { get; private set; }
    public IReadOnlyDictionary<string, Object> Objects => _objects.AsReadOnly();
    public IReadOnlyList<Expression> Axioms { get; }
    public IReadOnlyList<Expression> VerifiedConclusions => _verifiedConclusions.AsReadOnly();
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public State(IEnumerable<Expression> axioms)
    {
        var axiomList = axioms?.ToList() ?? throw new IAException<ArgumentNullException>(nameof(axioms));

        Phase = StatePhase.Initial;
        Axioms = axiomList.AsReadOnly();
        _verifiedConclusions = new List<Expression>();
        _facts = new HashSet<Expression>(axiomList);
        _objects = axiomList
            .Select(a => a.TargetObject)
            .GroupBy(o => o.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        CreatedAt = DateTime.UtcNow;
    }

    public bool ContainsFact(Expression expression)
    {
        if (expression is null)
            throw new IAException<ArgumentNullException>(nameof(expression));

        return _facts.Contains(expression);
    }

    /// <summary>
    /// Adds a verified conclusion exactly once and indexes its target object.
    /// </summary>
    public void AddVerifiedConclusion(Expression conclusion)
    {
        if (conclusion is null)
            throw new IAException<ArgumentNullException>(nameof(conclusion));

        if (_facts.Add(conclusion))
        {
            _verifiedConclusions.Add(conclusion);
            _objects.TryAdd(conclusion.TargetObject.Id, conclusion.TargetObject);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdatePhase(StatePhase newPhase)
    {
        Phase = newPhase;
        UpdatedAt = DateTime.UtcNow;
    }
}

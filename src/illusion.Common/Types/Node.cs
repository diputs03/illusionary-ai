namespace illusion.Common.Types;

/// <summary>
/// Base class for nodes in the knowledge graph
/// deterministic facts (Ground Truth), probabilistic experiences (Probabilistic).
/// </summary>
public abstract class Node
{
    /// <summary>
    /// Node type
    /// </summary>
    public enum NodeType
    {
        GroundTruth,
        Probabilistic
    }

    public string Id { get; }
    public string Name { get; }
    public NodeType Type { get; }
    public IReadOnlyList<Expression> Definitions { get; }
    public DateTime CreatedAt { get; }

    protected Node(string id, string name, NodeType type, IEnumerable<Expression> definitions)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("node ID cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("node name cannot be empty", nameof(name));

        Id = id;
        Name = name;
        Type = type;
        Definitions = definitions?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(definitions));
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// ground truth node (deterministic)
/// </summary>
public class GroundTruthNode : Node
{
    public string AxiomSystem { get; }

    public GroundTruthNode(string id, string name, string axiomSystem, IEnumerable<Expression> definitions)
        : base(id, name, NodeType.GroundTruth, definitions)
    {
        if (string.IsNullOrWhiteSpace(axiomSystem))
            throw new ArgumentException("axiom system cannot be empty", nameof(axiomSystem));
        AxiomSystem = axiomSystem;
    }
}

/// <summary>
/// probabilistic node (experience, uncertain knowledge)
/// </summary>
public class ProbabilisticNode : Node
{
    public double Confidence { get; private set; }

    public ProbabilisticNode(string id, string name, double initialConfidence, IEnumerable<Expression> definitions)
        : base(id, name, NodeType.Probabilistic, definitions)
    {
        if (initialConfidence < 0.0 || initialConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(initialConfidence), "confidence must be between 0.0 and 1.0");
        Confidence = initialConfidence;
    }

    public void UpdateConfidence(double newConfidence)
    {
        if (newConfidence < 0.0 || newConfidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(newConfidence));
        Confidence = newConfidence;
    }
}
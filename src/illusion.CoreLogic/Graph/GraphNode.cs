using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace illusion.CoreLogic.Graph;

/// <summary>
/// Immutable graph node representing a fact, predicate, or object in the knowledge base
/// Each node has a unique deterministic ID and type metadata
/// </summary>
public record GraphNode
{
    public string Id { get; init; } = "";
    public NodeType Type { get; init; }
    public string Label { get; init; } = "";
    
    [JsonIgnore]
    public FrozenDictionary<string, string> Properties { get; init; } = FrozenDictionary<string, string>.Empty;
    
    // For serialization
    public Dictionary<string, string> PropertiesSerializable
    {
        get => Properties.ToDictionary(kv => kv.Key, kv => kv.Value);
        init => Properties = value.ToFrozenDictionary();
    }
    
    public long CreatedAt { get; init; } = DateTime.UtcNow.Ticks;
    
    // For proof tracing
    public string? DerivedFromRule { get; init; }
    public List<string>? DerivedFromPremises { get; init; }

    public static GraphNode Create(string id, NodeType type, string label)
        => new() { Id = id, Type = type, Label = label };

    public static GraphNode Predicate(string name, string objectId)
        => new() { Id = $"{name}:{objectId}", Type = NodeType.Predicate, Label = $"{name}({objectId})" };

    public static GraphNode Object(string id, string name)
        => new() { Id = $"obj:{id}", Type = NodeType.Object, Label = name };

    public static GraphNode Rule(string ruleName)
        => new() { Id = $"rule:{ruleName}", Type = NodeType.Rule, Label = ruleName };

    public static GraphNode Axiom(string name)
        => new() { Id = $"axiom:{name}", Type = NodeType.Axiom, Label = name };
}

public enum NodeType
{
    Unknown,
    Object,          // Ground entity: Socrates, Plato
    Predicate,       // Unary predicate: Human(socrates), Mortal(socrates)
    Rule,            // Inference rule: Human->Mortal
    Axiom,           // Ground truth axiom
    ProofStep,       // Intermediate proof node
    Contradiction,   // Detected contradiction
    Equivalence      // Logical equivalence
}

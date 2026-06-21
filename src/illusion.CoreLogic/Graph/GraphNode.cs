using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace illusion.CoreLogic.Graph;

/// <summary>
/// Immutable graph node representing a fact, predicate, or object in the knowledge base.
/// IPK ALIGNED: No hardcoded NodeType enum - type is determined by rules, not baked in.
/// Each node has a unique deterministic ID and type metadata as string.
/// </summary>
public record GraphNode
{
    public string Id { get; init; } = "";
    
    /// <summary>
    /// Node type as string - determined by rules, not hardcoded enum.
    /// Common conventions: "Object", "Predicate", "Rule", "Axiom", "ProofStep", "Contradiction", "Equivalence"
    /// </summary>
    public string Type { get; init; } = "Unknown";
    
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

    /// <summary>
    /// Create a node with rule-determined type.
    /// </summary>
    public static GraphNode Create(string id, string type, string label)
        => new() { Id = id, Type = type, Label = label };

    /// <summary>
    /// Convention-based factory for predicate nodes.
    /// Type is "Predicate" by convention, not enforced by enum.
    /// </summary>
    public static GraphNode Predicate(string name, string objectId)
        => new() { Id = $"{name}:{objectId}", Type = "Predicate", Label = $"{name}({objectId})" };

    /// <summary>
    /// Convention-based factory for object nodes.
    /// Type is "Object" by convention, not enforced by enum.
    /// </summary>
    public static GraphNode Object(string id, string name)
        => new() { Id = $"obj:{id}", Type = "Object", Label = name };

    /// <summary>
    /// Convention-based factory for rule nodes.
    /// Type is "Rule" by convention, not enforced by enum.
    /// </summary>
    public static GraphNode Rule(string ruleName)
        => new() { Id = $"rule:{ruleName}", Type = "Rule", Label = ruleName };

    /// <summary>
    /// Convention-based factory for axiom nodes.
    /// Type is "Axiom" by convention, not enforced by enum.
    /// </summary>
    public static GraphNode Axiom(string name)
        => new() { Id = $"axiom:{name}", Type = "Axiom", Label = name };
}

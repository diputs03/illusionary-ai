namespace illusion.CoreLogic.Graph;

/// <summary>
/// Directed labeled edge between graph nodes.
/// IPK ALIGNED: No hardcoded EdgeType enum - relationship type is determined by rules.
/// Represents logical relationships: implication, derivation, membership, etc.
/// </summary>
public record GraphEdge
{
    public string FromId { get; init; } = "";
    public string ToId { get; init; } = "";
    
    /// <summary>
    /// Edge type as string - determined by rules, not hardcoded enum.
    /// Common conventions: "Implies", "DerivedFrom", "HasProperty", "InstanceOf", 
    /// "SubclassOf", "Equivalent", "Contradicts", "Causes", "CorrelatedWith", etc.
    /// </summary>
    public string Type { get; init; } = "Unknown";
    
    public string Label { get; init; } = "";
    public long Weight { get; init; } = 1;
    public long CreatedAt { get; init; } = DateTime.UtcNow.Ticks;

    /// <summary>
    /// Create an edge with rule-determined type.
    /// </summary>
    public static GraphEdge Create(string from, string to, string type, string label = "")
        => new() { FromId = from, ToId = to, Type = type, Label = label };

    /// <summary>
    /// Convention-based factory for logical implication.
    /// Type is "Implies" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge Implies(string from, string to)
        => new() { FromId = from, ToId = to, Type = "Implies", Label = "⊢" };

    /// <summary>
    /// Convention-based factory for proof derivation.
    /// Type is "DerivedFrom" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge DerivedFrom(string conclusion, string premise)
        => new() { FromId = conclusion, ToId = premise, Type = "DerivedFrom", Label = "⇐" };

    /// <summary>
    /// Convention-based factory for property assignment.
    /// Type is "HasProperty" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge HasProperty(string objId, string predicateId)
        => new() { FromId = objId, ToId = predicateId, Type = "HasProperty", Label = "has" };

    /// <summary>
    /// Convention-based factory for category membership.
    /// Type is "InstanceOf" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge InstanceOf(string instance, string category)
        => new() { FromId = instance, ToId = category, Type = "InstanceOf", Label = "∈" };

    /// <summary>
    /// Convention-based factory for taxonomy hierarchy.
    /// Type is "SubclassOf" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge SubclassOf(string subclass, string superclass)
        => new() { FromId = subclass, ToId = superclass, Type = "SubclassOf", Label = "⊑" };

    /// <summary>
    /// Convention-based factory for logical equivalence.
    /// Type is "Equivalent" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge Equivalent(string a, string b)
        => new() { FromId = a, ToId = b, Type = "Equivalent", Label = "≡" };

    /// <summary>
    /// Convention-based factory for contradiction.
    /// Type is "Contradicts" by convention, not enforced by enum.
    /// </summary>
    public static GraphEdge Contradicts(string a, string b)
        => new() { FromId = a, ToId = b, Type = "Contradicts", Label = "⊥" };
}

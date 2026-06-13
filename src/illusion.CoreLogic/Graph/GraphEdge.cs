namespace illusion.CoreLogic.Graph;

/// <summary>
/// Directed labeled edge between graph nodes
/// Represents logical relationships: implication, derivation, membership, etc.
/// </summary>
public record GraphEdge
{
    public string FromId { get; init; } = "";
    public string ToId { get; init; } = "";
    public EdgeType Type { get; init; }
    public string Label { get; init; } = "";
    public long Weight { get; init; } = 1;
    public long CreatedAt { get; init; } = DateTime.UtcNow.Ticks;

    // Factory methods for common relationship types
    public static GraphEdge Implies(string from, string to)
        => new() { FromId = from, ToId = to, Type = EdgeType.Implies, Label = "⊢" };

    public static GraphEdge DerivedFrom(string conclusion, string premise)
        => new() { FromId = conclusion, ToId = premise, Type = EdgeType.DerivedFrom, Label = "⇐" };

    public static GraphEdge HasProperty(string objId, string predicateId)
        => new() { FromId = objId, ToId = predicateId, Type = EdgeType.HasProperty, Label = "has" };

    public static GraphEdge InstanceOf(string instance, string category)
        => new() { FromId = instance, ToId = category, Type = EdgeType.InstanceOf, Label = "∈" };

    public static GraphEdge SubclassOf(string subclass, string superclass)
        => new() { FromId = subclass, ToId = superclass, Type = EdgeType.SubclassOf, Label = "⊑" };

    public static GraphEdge Equivalent(string a, string b)
        => new() { FromId = a, ToId = b, Type = EdgeType.Equivalent, Label = "≡" };

    public static GraphEdge Contradicts(string a, string b)
        => new() { FromId = a, ToId = b, Type = EdgeType.Contradicts, Label = "⊥" };
}

public enum EdgeType
{
    Unknown,
    Implies,           // Logical implication: A ⊢ B
    DerivedFrom,       // Proof derivation: B ⇐ A
    HasProperty,       // Object has predicate: Socrates has Human
    InstanceOf,        // Category membership: Socrates ∈ Human
    SubclassOf,        // Taxonomy: Human ⊑ Animal
    Equivalent,        // Logical equivalence: A ≡ B
    Contradicts,       // Inconsistency: A ⊥ B
    Causes,            // Causal relation
    CorrelatedWith,    // Statistical correlation
    TemporalBefore,    // Time ordering
    SpatialNear        // Spatial relation
}

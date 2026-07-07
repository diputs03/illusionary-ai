namespace illusion.Common.Types;

/// <summary>
/// Canonical object used by the E(O) predicate model.
/// </summary>
public sealed class Object : IEquatable<Object>
{
    public string Id { get; }
    public string Name { get; }

    public Object(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new IAException<ArgumentException>("object id cannot be empty");
        if (string.IsNullOrWhiteSpace(name))
            throw new IAException<ArgumentException>("object name cannot be empty");

        Id = id.Trim();
        Name = name.Trim();
    }

    public bool Equals(Object? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(Id, other.Id)
        && StringComparer.Ordinal.Equals(Name, other.Name);

    public override int GetHashCode() => HashCode.Combine(
        StringComparer.Ordinal.GetHashCode(Id),
        StringComparer.Ordinal.GetHashCode(Name));

    public override string ToString() => $"{Name}#{Id}";
}

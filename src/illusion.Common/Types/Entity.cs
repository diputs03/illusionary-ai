namespace illusion.Common.Types;

public class Entity
{
    public string Id { get; }
    public string Name { get; }

    public Entity(string id, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
namespace illusion.Common.Types;

public class Object
{
    public string Id { get; }
    public string Name { get; }

    public Object(string id, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
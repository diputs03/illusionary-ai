namespace illusion.Common.Types;

/// <summary>
/// dual action model
/// plan, search
/// </summary>
public class Action
{
    public enum ActionType
    {
        Plan,
        Search
    }

    public string Id { get; }
    public string Name { get; }
    public ActionType Type { get; }
    public IReadOnlyList<Types.Constraint> Constraints { get; }
    public string Description { get; }

    public Action(string id, string name, ActionType type, IEnumerable<Types.Constraint> constraints, string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        Type = type;
        Constraints = constraints?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(constraints));
        Description = description ?? string.Empty;
    }
}
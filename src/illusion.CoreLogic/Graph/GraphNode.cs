using illusion.CoreLogic.Kernel;
using System.Collections.Frozen;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace illusion.CoreLogic.Graph;

public abstract class Node
{
    public string Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string Label { get; set; } = string.Empty;
    protected string Ref { get; set; } = string.Empty;
    public JsonObject ToJson()
    {
        JsonObject obj = new JsonObject
        {
            ["id"] = Id,
            ["createdAt"] = CreatedAt,
            ["label"] = Label,
            ["ref"] = Ref
        };

        return obj;
    }
}
using System.Text.Json;

namespace illusion.Common.Utils;

/// <summary>
/// Safe serialization utilities using the .NET platform serializer. The methods
/// preserve the original byte[] contract while avoiding third-party serializer
/// vulnerabilities in the trusted core path.
/// </summary>
public static class SerializationUtils
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static byte[] Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return JsonSerializer.SerializeToUtf8Bytes(obj, Options);
    }

    public static T Deserialize<T>(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return JsonSerializer.Deserialize<T>(data, Options)
            ?? throw new InvalidOperationException($"failed to deserialize {typeof(T).FullName}");
    }
}

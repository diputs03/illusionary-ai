using MessagePack;

namespace illusion.Common.Utils;

/// <summary>
/// fast serialization utils
/// </summary>
public static class SerializationUtils
{
    static SerializationUtils()
    {
        // global MessagePack configuration: compatible with cross-language, secure
        MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
            .WithResolver(MessagePack.Resolvers.StandardResolver.Instance)
            .WithCompression(MessagePackCompression.Lz4BlockArray);
    }
    public static byte[] Serialize<T>(T obj)
    {
        return MessagePackSerializer.Serialize(obj);
    }
    public static T Deserialize<T>(byte[] data)
    {
        return MessagePackSerializer.Deserialize<T>(data);
    }
}
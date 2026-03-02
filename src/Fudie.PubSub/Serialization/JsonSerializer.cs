namespace Fudie.PubSub.Serialization;

/// <summary>
/// JSON-based implementation of <see cref="ISerializer"/> using System.Text.Json.
/// </summary>
public sealed class JsonPubSubSerializer : ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T value) =>
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] data) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(data)
            ?? throw new InvalidOperationException($"Failed to deserialize message to {typeof(T).Name}");
}

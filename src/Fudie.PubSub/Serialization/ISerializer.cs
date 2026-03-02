namespace Fudie.PubSub.Serialization;

/// <summary>
/// Defines serialization and deserialization operations for message transport.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Serializes the specified value to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>A byte array containing the serialized data.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes a byte array into an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="data">The byte array to deserialize.</param>
    /// <returns>The deserialized instance.</returns>
    T Deserialize<T>(byte[] data);
}

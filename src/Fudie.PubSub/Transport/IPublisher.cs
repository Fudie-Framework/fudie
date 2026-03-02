namespace Fudie.PubSub.Transport;

/// <summary>
/// Publishes messages to a topic at the transport level.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <param name="topicId">The topic identifier to publish to.</param>
    /// <param name="message">The message to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAsync<T>(string topicId, T message);
}

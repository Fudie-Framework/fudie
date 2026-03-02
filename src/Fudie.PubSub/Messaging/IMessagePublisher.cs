namespace Fudie.PubSub.Messaging;

/// <summary>
/// Publishes messages to a specified topic.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <typeparam name="T">The type of the message payload.</typeparam>
    /// <param name="topicId">The topic identifier to publish to.</param>
    /// <param name="message">The message payload to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAsync<T>(string topicId, T message);
}

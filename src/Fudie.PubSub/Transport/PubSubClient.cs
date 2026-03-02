namespace Fudie.PubSub.Transport;

/// <summary>
/// Abstract base class for pub/sub clients that handles argument validation, serialization,
/// and delegates transport-specific operations to derived classes.
/// </summary>
/// <param name="serializer">An optional serializer; defaults to <see cref="JsonPubSubSerializer"/> if not provided.</param>
public abstract class PubSubClient(ISerializer? serializer = null) : IPubSubClient
{
    private readonly ISerializer _serializer = serializer ?? new JsonPubSubSerializer();

    /// <inheritdoc/>
    Task ITopicAdmin.CreateTopicAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return CreateTopicAsync(topicId);
    }

    /// <inheritdoc/>
    Task ITopicAdmin.DeleteTopicAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return DeleteTopicAsync(topicId);
    }

    /// <inheritdoc/>
    Task<bool> ITopicAdmin.TopicExistsAsync(string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return TopicExistsAsync(topicId);
    }

    /// <inheritdoc/>
    Task ISubscriptionAdmin.CreateSubscriptionAsync(string subscriptionId, string topicId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        return CreateSubscriptionAsync(subscriptionId, topicId);
    }

    /// <inheritdoc/>
    Task ISubscriptionAdmin.DeleteSubscriptionAsync(string subscriptionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        return DeleteSubscriptionAsync(subscriptionId);
    }

    /// <inheritdoc/>
    Task<bool> ISubscriptionAdmin.SubscriptionExistsAsync(string subscriptionId)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        return SubscriptionExistsAsync(subscriptionId);
    }

    /// <inheritdoc/>
    Task IPublisher.PublishAsync<T>(string topicId, T message)
    {
        ArgumentException.ThrowIfNullOrEmpty(topicId);
        ArgumentNullException.ThrowIfNull(message);
        var data = _serializer.Serialize(message);
        return PublishAsync(topicId, data);
    }

    /// <inheritdoc/>
    Task ISubscriber.SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeAsync(subscriptionId, (data, token) =>
        {
            var message = _serializer.Deserialize<T>(data);
            return handler(message, token);
        }, ct);
    }

    /// <summary>
    /// Creates a topic in the underlying transport.
    /// </summary>
    /// <param name="topicId">The validated topic identifier.</param>
    protected abstract Task CreateTopicAsync(string topicId);

    /// <summary>
    /// Deletes a topic from the underlying transport.
    /// </summary>
    /// <param name="topicId">The validated topic identifier.</param>
    protected abstract Task DeleteTopicAsync(string topicId);

    /// <summary>
    /// Checks whether a topic exists in the underlying transport.
    /// </summary>
    /// <param name="topicId">The validated topic identifier.</param>
    protected abstract Task<bool> TopicExistsAsync(string topicId);

    /// <summary>
    /// Creates a subscription in the underlying transport.
    /// </summary>
    /// <param name="subscriptionId">The validated subscription identifier.</param>
    /// <param name="topicId">The validated topic identifier.</param>
    protected abstract Task CreateSubscriptionAsync(string subscriptionId, string topicId);

    /// <summary>
    /// Deletes a subscription from the underlying transport.
    /// </summary>
    /// <param name="subscriptionId">The validated subscription identifier.</param>
    protected abstract Task DeleteSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Checks whether a subscription exists in the underlying transport.
    /// </summary>
    /// <param name="subscriptionId">The validated subscription identifier.</param>
    protected abstract Task<bool> SubscriptionExistsAsync(string subscriptionId);

    /// <summary>
    /// Publishes serialized data to a topic in the underlying transport.
    /// </summary>
    /// <param name="topicId">The validated topic identifier.</param>
    /// <param name="data">The serialized message data.</param>
    protected abstract Task PublishAsync(string topicId, byte[] data);

    /// <summary>
    /// Subscribes to raw messages from the underlying transport.
    /// </summary>
    /// <param name="subscriptionId">The validated subscription identifier.</param>
    /// <param name="handler">The handler invoked with raw message data.</param>
    /// <param name="ct">A cancellation token to observe.</param>
    protected abstract Task SubscribeAsync(string subscriptionId, Func<byte[], CancellationToken, Task> handler, CancellationToken ct);
}

namespace Fudie.PubSub.Messaging;

/// <summary>
/// Wraps messages in an <see cref="Envelope{T}"/> with context metadata and publishes them to a topic.
/// </summary>
/// <param name="client">The pub/sub client used for publishing.</param>
/// <param name="context">The current message context providing correlation and claims data.</param>
public class MessagePublisher(IPubSubClient client, IMessageContext context) : IMessagePublisher
{
    /// <inheritdoc/>
    public Task PublishAsync<T>(string topicId, T message)
    {
        var envelope = new Envelope<T>(
            MessageId: Guid.NewGuid().ToString(),
            CorrelationId: context.CorrelationId,
            Type: typeof(T).Name,
            OccurredAt: DateTime.UtcNow,
            Claims: context.Claims.Count > 0 ? new Dictionary<string, string>(context.Claims) : null,
            Payload: message
        );

        IPublisher publisher = client;
        return publisher.PublishAsync(topicId, envelope);
    }
}

namespace Fudie.PubSub.Hosting;

/// <summary>
/// Manages message subscriptions and dispatches incoming messages to their handlers.
/// </summary>
/// <param name="client">The pub/sub client used for subscribing.</param>
/// <param name="serviceProvider">The service provider used to resolve scoped handlers.</param>
public class MessageHost(IPubSubClient client, IServiceProvider serviceProvider)
{
    /// <summary>
    /// Subscribes to a subscription and dispatches received messages to the registered <see cref="IMessageHandler{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of message payload.</typeparam>
    /// <param name="subscriptionId">The subscription identifier to listen on.</param>
    /// <param name="ct">A cancellation token to observe.</param>
    /// <returns>A task representing the subscription lifetime.</returns>
    public Task SubscribeAsync<T>(string subscriptionId, CancellationToken ct = default)
    {
        ISubscriber subscriber = client;

        return subscriber.SubscribeAsync<Envelope<T>>(subscriptionId, async (envelope, token) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<MessageContext>();
            context.Populate(envelope.Claims, envelope.CorrelationId);

            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
            await handler.Handle(envelope.Payload, token);
        }, ct);
    }
}

namespace Fudie.PubSub.Transport;

/// <summary>
/// Subscribes to messages from a subscription at the transport level.
/// </summary>
public interface ISubscriber
{
    /// <summary>
    /// Subscribes to a subscription and invokes the handler for each received message.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <param name="subscriptionId">The subscription identifier to listen on.</param>
    /// <param name="handler">The handler invoked for each received message.</param>
    /// <param name="ct">A cancellation token to observe.</param>
    /// <returns>A task representing the subscription lifetime.</returns>
    Task SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler, CancellationToken ct = default);
}

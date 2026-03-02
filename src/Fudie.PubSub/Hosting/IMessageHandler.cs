namespace Fudie.PubSub.Hosting;

/// <summary>
/// Defines a handler that processes messages of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of message to handle.</typeparam>
public interface IMessageHandler<in T>
{
    /// <summary>
    /// Handles the specified message.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="ct">A cancellation token to observe.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(T message, CancellationToken ct);
}

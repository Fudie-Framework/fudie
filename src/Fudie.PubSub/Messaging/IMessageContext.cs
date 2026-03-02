namespace Fudie.PubSub.Messaging;

/// <summary>
/// Provides access to contextual information about the current message being processed.
/// </summary>
public interface IMessageContext
{
    /// <summary>
    /// Gets the tenant identifier extracted from the message claims.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the user identifier extracted from the message claims.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracing related messages.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the claims associated with the message sender.
    /// </summary>
    IDictionary<string, string> Claims { get; }
}

namespace Fudie.PubSub.Messaging;

/// <summary>
/// Scoped implementation of <see cref="IMessageContext"/> that stores contextual information
/// extracted from an incoming message envelope.
/// </summary>
public class MessageContext : IMessageContext
{
    /// <inheritdoc/>
    public string? TenantId { get; private set; }

    /// <inheritdoc/>
    public string? UserId { get; private set; }

    /// <inheritdoc/>
    public string? CorrelationId { get; private set; }

    /// <inheritdoc/>
    public IDictionary<string, string> Claims { get; private set; } = new Dictionary<string, string>();

    /// <summary>
    /// Populates the context with data extracted from the message envelope.
    /// </summary>
    /// <param name="claims">The claims dictionary from the envelope, or <c>null</c> if none.</param>
    /// <param name="correlationId">The correlation identifier from the envelope.</param>
    public void Populate(IDictionary<string, string>? claims, string? correlationId)
    {
        CorrelationId = correlationId;

        if (claims is null) return;

        Claims = new Dictionary<string, string>(claims);
        claims.TryGetValue("tenant_id", out var tenantId);
        TenantId = tenantId;
        claims.TryGetValue("sub", out var userId);
        UserId = userId;
    }
}

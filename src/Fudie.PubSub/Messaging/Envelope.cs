namespace Fudie.PubSub.Messaging;

/// <summary>
/// Wraps a message payload with metadata for transport.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
/// <param name="MessageId">The unique identifier for this message.</param>
/// <param name="CorrelationId">An optional correlation identifier for tracing related messages.</param>
/// <param name="Type">The message type name.</param>
/// <param name="OccurredAt">The timestamp when the message was created.</param>
/// <param name="Claims">Optional claims associated with the message sender.</param>
/// <param name="Payload">The message payload.</param>
public record Envelope<T>(
    string MessageId,
    string? CorrelationId,
    string Type,
    DateTime OccurredAt,
    IDictionary<string, string>? Claims,
    T Payload
);

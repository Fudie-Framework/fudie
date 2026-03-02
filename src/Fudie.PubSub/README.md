# Fudie.PubSub

Messaging core for the Fudie platform. Defines contracts, abstractions, and the hosting layer. **Does not contain any provider implementation**.

## Structure

```
Fudie.PubSub/
  Transport/        Transport contracts (publish, subscribe, admin)
  Serialization/    Serialization contract and default implementation
  Messaging/        Envelope, message context, and high-level publisher
  Hosting/          Handler, MessageHost, and DI configuration
```

## Transport

Segregated interfaces for transport operations:

| Interface | Responsibility |
|-----------|----------------|
| `IPublisher` | Publish messages to a topic |
| `ISubscriber` | Subscribe to messages from a subscription |
| `ITopicAdmin` | Create, delete, and check topics |
| `ISubscriptionAdmin` | Create, delete, and check subscriptions |
| `IPubSubClient` | Composite interface grouping all of the above |

`PubSubClient` is the abstract base class implementing `IPubSubClient` with argument validation. Providers inherit from it.

## Serialization

```csharp
public interface ISerializer
{
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] data);
}
```

`JsonPubSubSerializer` is the default implementation using `System.Text.Json`. If no `ISerializer` is registered in DI, the provider uses this implementation automatically.

## Messaging

### Envelope

Each message travels wrapped in an `Envelope<T>`:

```csharp
public record Envelope<T>(
    string MessageId,
    string? CorrelationId,
    string Type,
    DateTime OccurredAt,
    IDictionary<string, string>? Claims,
    T Payload
);
```

Developers never interact with the envelope directly. `MessagePublisher` builds it when publishing and `MessageHost` unwraps it when receiving.

### IMessageContext

Scoped context exposing data from the current message:

```csharp
public interface IMessageContext
{
    string? TenantId { get; }
    string? UserId { get; }
    string? CorrelationId { get; }
    IDictionary<string, string> Claims { get; }
}
```

Injected into handlers, DbContexts (for multi-tenant QueryFilters), and any scoped service. Automatically populated from:

- **HTTP middleware** (JWT claims)
- **Outbox worker** (stored claims)
- **MessageHost** (envelope claims when receiving a message)

### MessagePublisher

`IMessagePublisher` is the high-level interface for publishing messages:

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<T>(string topicId, T message);
}
```

Wraps `T` in `Envelope<T>` using claims from the current `IMessageContext`.

## Hosting

### IMessageHandler\<T\>

Contract for consuming messages:

```csharp
public interface IMessageHandler<in T>
{
    Task Handle(T message, CancellationToken ct);
}
```

### MessageHost

Orchestrates message reception. For each message:

1. Creates a DI scope
2. Populates `MessageContext` with claims and correlationId from the envelope
3. Resolves `IMessageHandler<T>` within the scope
4. Executes the handler

This ensures each message has its own scope with independent `IMessageContext` and `DbContext`.

### Configuration

```csharp
builder.Services.AddPubSubMessaging(pubsub =>
{
    pubsub.UseGcp(builder.Configuration);  // provider extension
});
```

`AddPubSubMessaging` registers:

| Service | Lifetime |
|---------|----------|
| `MessageContext` / `IMessageContext` | Scoped |
| `IMessagePublisher` | Scoped |
| `MessageHost` | Singleton |

The provider (e.g. `UseGcp`) registers `IPubSubClient` as Singleton.

## Dependencies

This project **has no provider dependencies**. It only depends on:

- `Microsoft.Extensions.DependencyInjection.Abstractions`

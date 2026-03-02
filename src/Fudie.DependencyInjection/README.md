# Fudie.DependencyInjection

Attribute-based automatic dependency injection registration via reflection-based assembly scanning.

## Usage

### 1. Mark your classes

```csharp
[Injectable]  // Defaults to Scoped lifetime
public class OrderService : IOrderService { }

[Injectable(ServiceLifetime.Singleton)]
public class CacheService : ICacheService { }

[Injectable(ServiceLifetime.Transient)]
public class NotificationFactory : INotificationFactory { }
```

### 2. Register at startup

```csharp
builder.Services.AddInjectables(typeof(OrderService).Assembly);
```

This scans the assembly for all classes decorated with `[Injectable]` and registers them with their implemented interfaces using the specified lifetime.

## Public Types

| Type | Description |
|------|-------------|
| `InjectableAttribute` | Attribute to mark classes for automatic DI registration |
| `InjectionExtension` | Extension methods for `IServiceCollection` to register injectables |

## Dependencies

- `Microsoft.Extensions.DependencyInjection.Abstractions`

# Fudie.Domain

Core domain-driven design building blocks for entity modeling, domain events, result patterns, and base command types.

## Public Types

### Entities

| Type | Description |
|------|-------------|
| `Entity<TId>` | Base entity with identity-based equality |
| `AggregateRoot<TId>` | Aggregate root with domain event collection |
| `IDomainEvent` | Marker interface for domain events |
| `IHasDomainEvents` | Interface exposing domain events and clear mechanism |

### Result Pattern

| Type | Description |
|------|-------------|
| `Result` | Success/failure result without value |
| `Result<T>` | Success/failure result with value |
| `ValidationError` | Validation error with field name and message |

### Commands

| Type | Description |
|------|-------------|
| `AbstractCreateCommand<TCommand, TEntity>` | Base for entity creation commands |
| `AbstractModifyCommand<TEntity>` | Base for entity modification commands |
| `AbstractModifyCommand<TCommand, TEntity>` | Base for entity modification with command input |
| `AbstractTransformCommand<TCommand, TValueObject>` | Base for value object transformation commands |
| `AbstractTransformCommand<TValueObject>` | Base for parameterless value object transformations |

### Exceptions

| Type | HTTP Status |
|------|-------------|
| `ConflictException` | 409 Conflict |
| `UnauthorizedException` | 401 Unauthorized |

Both exceptions support an optional `ErrorCode` property for traceability:

```csharp
throw new ConflictException("Slug already taken", "Customer.Slug.AlreadyExists");
throw new UnauthorizedException("Not the owner", "Order.Owner.NotOwner");

// Legacy usage (without error code) still works
throw new ConflictException("Slug already taken");
```

## Dependencies

None. This is a pure domain library with zero external dependencies.

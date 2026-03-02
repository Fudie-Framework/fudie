# Fudie.Validation

Guard patterns and extension methods for validation, exception handling, result unwrapping, and entity lookup validation.

## Guards

### ValidationGuard

```csharp
ValidationGuard.ThrowIf(name == "", "Name", "Name is required");
ValidationGuard.ThrowIfNot(age > 0, "Age", "Age must be positive");
```

Throws `FluentValidation.ValidationException` with structured errors.

### NotFoundGuard

```csharp
NotFoundGuard.ThrowIfNull(entity, entityId);
NotFoundGuard.ThrowIfNull<Order>(entity);
NotFoundGuard.ThrowIfNull<Order, Guid>(entity, orderId);
```

Throws `KeyNotFoundException` with formatted messages like `"Order with id 'xxx' was not found"`.

### ConflictGuard

```csharp
ConflictGuard.ThrowIf(isDuplicate, "An order with this number already exists");
```

Throws `ConflictException`.

### UnauthorizedGuard

```csharp
UnauthorizedGuard.ThrowIf(!isOwner, "Only owners can perform this action");
```

Throws `UnauthorizedException`.

## Result Extensions

```csharp
Result<Order> result = CreateOrder(command);
Order order = result.ValueOrThrow();   // Throws ValidationException if failed
result.SuccessOrThrow();                // Throws ValidationException if failed (no value)
```

## Validator Extensions

```csharp
await validator.ValidateOrThrow(command);
```

Validates using FluentValidation and throws on failure. Also checks for empty Guids.

## Public Types

| Type | Description |
|------|-------------|
| `ValidationGuard` | Condition-based validation with `ThrowIf` / `ThrowIfNot` |
| `NotFoundGuard` | Null-check guards throwing `KeyNotFoundException` |
| `ConflictGuard` | Conflict condition guard throwing `ConflictException` |
| `UnauthorizedGuard` | Authorization guard throwing `UnauthorizedException` |
| `ResultExtensions` | `ValueOrThrow()` / `SuccessOrThrow()` for `Result<T>` |
| `ValidatorExtensions` | `ValidateOrThrow<T>()` for FluentValidation validators |

## Dependencies

- `FluentValidation`
- `Fudie.Domain`

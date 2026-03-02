# Fudie.Infrastructure

Entity Framework Core abstractions and base context for database operations, repository patterns, change tracking, and domain event collection.

## Repository Interfaces

| Interface | Description |
|-----------|-------------|
| `IGet<T, ID>` | Read entity by ID |
| `IAdd<T>` | Insert entity |
| `IUpdate<T, ID>` | Update entity (inherits `IGet`) |
| `IRemove<T, ID>` | Remove entity (inherits `IGet`) |
| `IQuery` | Queryable access for read-only operations |
| `IEntityLookup` | Tracked entity retrieval with include/filter options |
| `IUnitOfWork` | `SaveChangesAsync()` abstraction |
| `IChangeTracker` | Access to EF change tracking entries |

## FudieDbContext

Abstract base `DbContext` implementing all repository interfaces. Features:

- Auto-ignores `DomainEvents` navigation property
- Collects and clears domain events on `SaveChangesAsync()`
- Implements `IQuery`, `IEntityLookup`, `IUnitOfWork`, `IChangeTracker`

```csharp
public class AppDbContext : FudieDbContext
{
    public DbSet<Order> Orders => Set<Order>();
}
```

## Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `GenerateRepository<TEntity>` | Interface | Triggers source generation for the repository |
| `GenerateRepository<TEntity, TId>` | Interface | Same, with explicit ID type |
| `Include<TEntity>` | Interface | Configures eager loading paths |
| `Tracking` | Interface/Method | Enables change tracking |
| `AsNoTracking` | Interface/Method | Disables change tracking |
| `AsSplitQuery` | Interface/Method | Uses split queries |
| `IgnoreQueryFilters` | Interface/Method | Ignores global query filters |

## Dependencies

- `Microsoft.EntityFrameworkCore`
- `Fudie.Domain`

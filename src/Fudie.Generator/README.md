# Fudie.Generator

Source Generator for automatic repository implementations with Entity Framework Core.

## Table of Contents

- [Base Interfaces](#base-interfaces)
- [Attributes](#attributes)
- [Query Method Conventions](#query-method-conventions)
  - [Query Prefixes](#query-prefixes)
  - [Operators](#operators)
  - [Modifiers](#modifiers)
  - [Full Examples](#full-examples)
- [Tracking](#tracking)

---

## Base Interfaces

The generator recognizes the following infrastructure interfaces:

| Interface | Description | Generated Method |
|-----------|-------------|-----------------|
| `IGet<T, ID>` | Read by ID | `Task<T> Get(ID id)` |
| `IAdd<T>` | Insert | `void Add(T entity)` |
| `IUpdate<T, ID>` | Update (inherits IGet) | `Task<T> Get(ID id)` with tracking |
| `IRemove<T, ID>` | Delete (inherits IGet) | `void Remove(T entity)` |

### Basic Example

```csharp
public interface ICustomerRepository : IGet<Customer, Guid>, IAdd<Customer>
{
}
// Generates: Get(Guid id) and Add(Customer entity)
```

---

## Attributes

### `[GenerateRepository<TEntity>]` / `[GenerateRepository<TEntity, TId>]`

Allows creating query-only repositories without exposing unsafe `Get(id)` methods.

```csharp
[GenerateRepository<Ingredient>]
public interface IIngredientQueries
{
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);
}
```

### `[Include<TEntity>("path", ...)]`

Configures eager loading with Include/ThenInclude.

```csharp
[Include<Customer>("Orders.OrderItems.Product", "Address")]
public interface ICustomerRepository : IGet<Customer, Guid> { }
```

**Generated LINQ:**
```csharp
query = query.Include(c => c.Orders)
    .ThenInclude(o => o.OrderItems)
    .ThenInclude(oi => oi.Product);
query = query.Include(c => c.Address);
```

### `[Tracking]` / `[Tracking(bool)]`

Enables change tracking. Applicable to **interfaces** and **methods**.

### `[AsNoTracking]`

Disables change tracking. Applicable to **interfaces** and **methods**.

### `[AsSplitQuery]`

Uses split queries to avoid cartesian explosion.

### `[IgnoreQueryFilters]`

Ignores global query filters (e.g. soft delete).

---

## Query Method Conventions

The generator parses method names and generates LINQ code automatically.

### Query Prefixes

| Prefix | Return Type | Final LINQ |
|--------|-------------|------------|
| `FindBy` | `Task<List<T>>` | `.ToListAsync()` |
| `FindFirstBy` | `Task<T?>` | `.FirstOrDefaultAsync()` |
| `FindTop{N}By` | `Task<List<T>>` | `.Take(N).ToListAsync()` |
| `CountBy` | `Task<int>` | `.CountAsync()` |
| `ExistsBy` | `Task<bool>` | `.AnyAsync()` |
| `DeleteBy` | `Task<int>` | `.ExecuteDeleteAsync()` |

---

### Operators

| Operator | Method Usage | Generated LINQ |
|----------|-------------|----------------|
| *(none)* / `Equal` | `FindByName` | `x.Name == name` |
| `NotEqual` | `FindByStatusNotEqual` | `x.Status != status` |
| `LessThan` | `FindByPriceLessThan` | `x.Price < price` |
| `LessThanOrEqual` | `FindByAgeLessThanOrEqual` | `x.Age <= age` |
| `GreaterThan` | `FindByScoreGreaterThan` | `x.Score > score` |
| `GreaterThanOrEqual` | `FindByDateGreaterThanOrEqual` | `x.Date >= date` |
| `Between` | `FindByPriceBetween` | `x.Price >= min && x.Price <= max` |
| `In` | `FindByStatusIn` | `statuses.Contains(x.Status)` |
| `NotIn` | `FindByTypeNotIn` | `!types.Contains(x.Type)` |
| `StartsWith` | `FindByNameStartsWith` | `x.Name.StartsWith(prefix)` |
| `EndsWith` | `FindByEmailEndsWith` | `x.Email.EndsWith(suffix)` |
| `Contains` | `FindByDescriptionContains` | `x.Description.Contains(text)` |
| `Like` | `FindByNameLike` | `EF.Functions.Like(x.Name, pattern)` |
| `IsNull` | `FindByDeletedAtIsNull` | `x.DeletedAt == null` |
| `IsNotNull` | `FindByManagerIsNotNull` | `x.Manager != null` |
| `True` | `FindByIsActiveTrue` | `x.IsActive == true` |
| `False` | `FindByIsDeletedFalse` | `x.IsDeleted == false` |

---

### Modifiers

#### Logical Connectors

| Modifier | Usage | LINQ |
|----------|-------|------|
| `And` | `FindByNameAndAge` | `x.Name == name && x.Age == age` |
| `Or` | `FindByNameOrEmail` | `x.Name == name \|\| x.Email == email` |

#### Case Insensitive

| Modifier | Usage | LINQ |
|----------|-------|------|
| `IgnoreCase` | `FindByNameIgnoreCase` | `x.Name.ToLower() == name.ToLower()` |

#### Ordering

| Modifier | Usage | LINQ |
|----------|-------|------|
| `OrderBy{Prop}` | `FindByStatusOrderByName` | `.OrderBy(x => x.Name)` |
| `OrderBy{Prop}Asc` | `FindByStatusOrderByNameAsc` | `.OrderBy(x => x.Name)` |
| `OrderBy{Prop}Desc` | `FindByStatusOrderByNameDesc` | `.OrderByDescending(x => x.Name)` |

---

### Full Examples

#### 1. Simple Search

```csharp
Task<List<Customer>> FindByName(string name);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name == name)
    .ToListAsync();
```

---

#### 2. First Result with Two Conditions

```csharp
Task<Customer?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Id == id && x.RestaurantId == restaurantId)
    .FirstOrDefaultAsync();
```

---

#### 3. Search with OR

```csharp
Task<List<User>> FindByEmailOrPhone(string email, string phone);
```
**LINQ:**
```csharp
return await _query.Query<User>()
    .Where(x => x.Email == email || x.Phone == phone)
    .ToListAsync();
```

---

#### 4. Value Range

```csharp
Task<List<Product>> FindByPriceBetween(decimal min, decimal max);
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.Price >= min && x.Price <= max)
    .ToListAsync();
```

---

#### 5. Values in List

```csharp
Task<List<Order>> FindByStatusIn(List<OrderStatus> statuses);
```
**LINQ:**
```csharp
return await _query.Query<Order>()
    .Where(x => statuses.Contains(x.Status))
    .ToListAsync();
```

---

#### 6. Case Insensitive

```csharp
Task<List<Customer>> FindByNameIgnoreCase(string name);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name.ToLower() == name.ToLower())
    .ToListAsync();
```

---

#### 7. With Ordering

```csharp
Task<List<Product>> FindByCategoryOrderByPriceDesc(string category);
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.Category == category)
    .OrderByDescending(x => x.Price)
    .ToListAsync();
```

---

#### 8. Top N Results

```csharp
Task<List<Product>> FindTop5ByIsActiveTrue();
```
**LINQ:**
```csharp
return await _query.Query<Product>()
    .Where(x => x.IsActive == true)
    .Take(5)
    .ToListAsync();
```

---

#### 9. Count with Condition

```csharp
Task<int> CountByRestaurantIdAndIsActiveTrue(Guid restaurantId);
```
**LINQ:**
```csharp
return await _query.Query<Ingredient>()
    .Where(x => x.RestaurantId == restaurantId && x.IsActive == true)
    .CountAsync();
```

---

#### 10. Check Existence

```csharp
Task<bool> ExistsByEmailIgnoreCase(string email);
```
**LINQ:**
```csharp
return await _query.Query<User>()
    .Where(x => x.Email.ToLower() == email.ToLower())
    .AnyAsync();
```

---

#### 11. Bulk Delete

```csharp
Task<int> DeleteByIsDeletedTrueAndDeletedAtLessThan(DateTime cutoff);
```
**LINQ:**
```csharp
return await _query.Query<AuditLog>()
    .Where(x => x.IsDeleted == true && x.DeletedAt < cutoff)
    .ExecuteDeleteAsync();
```

---

#### 12. String Operations

```csharp
Task<List<Customer>> FindByNameStartsWithAndEmailEndsWith(string prefix, string domain);
```
**LINQ:**
```csharp
return await _query.Query<Customer>()
    .Where(x => x.Name.StartsWith(prefix) && x.Email.EndsWith(domain))
    .ToListAsync();
```

---

#### 13. Null Checks

```csharp
Task<List<Employee>> FindByManagerIsNullAndDepartmentIsNotNull();
```
**LINQ:**
```csharp
return await _query.Query<Employee>()
    .Where(x => x.Manager == null && x.Department != null)
    .ToListAsync();
```

---

## Tracking

Tracking can be configured at the **interface** or **method** level. Method-level attributes take priority.

### Priority Rules

1. **Method attribute** -> Always wins
2. **Interface attribute** -> Default for methods without attribute
3. **No attributes** -> Default is `AsNoTracking` (no tracking)

### Generated Dependencies

| Configuration | Injected Dependency | Query Source |
|---------------|---------------------|--------------|
| No tracking | `IQuery` | `_query.Query<T>()` |
| With tracking | `IEntityLookup` | `_entityLookup.Set<T>()` |
| Mixed | Both | Per method |

### Example: Mixed Tracking

```csharp
[GenerateRepository<Ingredient>]
public interface IIngredientRepository
{
    // No tracking - read only
    [AsNoTracking]
    Task<List<Ingredient>> FindByName(string name);

    // With tracking - for later modification
    [Tracking]
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
}
```

**Generated Code:**
```csharp
public class IngredientRepository : IIngredientRepository
{
    private readonly IEntityLookup _entityLookup;  // For tracked methods
    private readonly IQuery _query;                 // For untracked methods

    public IngredientRepository(IEntityLookup entityLookup, IQuery query)
    {
        _entityLookup = entityLookup;
        _query = query;
    }

    public async Task<List<Ingredient>> FindByName(string name)
    {
        return await _query.Query<Ingredient>()  // No tracking
            .Where(x => x.Name == name)
            .ToListAsync();
    }

    public async Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId)
    {
        return await _entityLookup.Set<Ingredient>()  // With tracking
            .Where(x => x.Id == id && x.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();
    }
}
```

### Example: Interface-Level Tracking

```csharp
[GenerateRepository<Ingredient>]
[AsNoTracking]  // Default for all methods
public interface IIngredientReadRepository
{
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);

    [Tracking]  // Override: this method DOES use tracking
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
}
```

---

## Multi-Tenant Use Cases

For security in multi-tenant applications, use `[GenerateRepository]` instead of `IGet`:

```csharp
// BAD - Get(id) does not filter by tenant
public interface IIngredientRepository : IGet<Ingredient, Guid> { }

// GOOD - Only exposes methods that require tenant
[GenerateRepository<Ingredient>]
public interface IIngredientRepository
{
    Task<Ingredient?> FindFirstByIdAndRestaurantId(Guid id, Guid restaurantId);
    Task<List<Ingredient>> FindByRestaurantId(Guid restaurantId);
}
```

---

## Summary: Method -> LINQ

| Method | Generated LINQ |
|--------|----------------|
| `FindByX(v)` | `.Where(x => x.X == v).ToListAsync()` |
| `FindFirstByX(v)` | `.Where(x => x.X == v).FirstOrDefaultAsync()` |
| `FindTop10ByX(v)` | `.Where(x => x.X == v).Take(10).ToListAsync()` |
| `CountByX(v)` | `.Where(x => x.X == v).CountAsync()` |
| `ExistsByX(v)` | `.Where(x => x.X == v).AnyAsync()` |
| `DeleteByX(v)` | `.Where(x => x.X == v).ExecuteDeleteAsync()` |
| `FindByXAndY(x,y)` | `.Where(x => x.X == x && x.Y == y)...` |
| `FindByXOrY(x,y)` | `.Where(x => x.X == x \|\| x.Y == y)...` |
| `FindByXOrderByY(x)` | `.Where(...).OrderBy(x => x.Y)...` |
| `FindByXOrderByYDesc(x)` | `.Where(...).OrderByDescending(x => x.Y)...` |
| `FindByXIgnoreCase(x)` | `.Where(x => x.X.ToLower() == x.ToLower())...` |
| `FindByXBetween(min,max)` | `.Where(x => x.X >= min && x.X <= max)...` |
| `FindByXIn(list)` | `.Where(x => list.Contains(x.X))...` |
| `FindByXIsNull()` | `.Where(x => x.X == null)...` |
| `FindByXIsNotNull()` | `.Where(x => x.X != null)...` |
| `FindByXTrue()` | `.Where(x => x.X == true)...` |
| `FindByXFalse()` | `.Where(x => x.X == false)...` |
| `FindByXStartsWith(p)` | `.Where(x => x.X.StartsWith(p))...` |
| `FindByXEndsWith(s)` | `.Where(x => x.X.EndsWith(s))...` |
| `FindByXContains(t)` | `.Where(x => x.X.Contains(t))...` |
| `FindByXLike(p)` | `.Where(x => EF.Functions.Like(x.X, p))...` |
| `FindByXGreaterThan(v)` | `.Where(x => x.X > v)...` |
| `FindByXLessThanOrEqual(v)` | `.Where(x => x.X <= v)...` |

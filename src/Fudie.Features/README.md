# Fudie.Features

Feature module discovery and endpoint registration contracts for the modular architecture. Provides endpoint-to-aggregate mapping and catalog registry.

## Usage

### Define a feature module

```csharp
public class OrderFeature : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", GetOrders);
        app.MapPost("/orders", CreateOrder);
    }
}
```

### Describe an aggregate for the catalog

```csharp
public class OrderDescription : IAggregateDescription
{
    public string Id => "orders";
    public string DisplayName => "Orders";
    public string Icon => "shopping-cart";
}
```

### Map features at startup

```csharp
app.MapFudieFeatures();
```

This discovers all `IFeatureModule` implementations and registers their routes.

## Public Types

| Type | Description |
|------|-------------|
| `IFeatureModule` | Interface for feature endpoint registration via `AddRoutes()` |
| `IAggregateDescription` | Interface describing aggregate metadata (Id, DisplayName, Icon) |
| `ICatalogRegistry` | Interface for registering and querying endpoint catalog entries |
| `CatalogEntry` | Record with full endpoint metadata (verb, route, auth, aggregate, scope) |
| `CatalogDescription` | Metadata attribute for endpoint descriptions |
| `RouteExtension` | Extension methods for discovering and mapping feature modules |

## Dependencies

- `Microsoft.AspNetCore.App` (framework reference)

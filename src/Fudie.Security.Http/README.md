# Fudie.Security.Http

Authorization middleware, endpoint catalog, and security requirements for HTTP APIs.

## Usage

```csharp
app.MapFudieFeatures(builder =>
{
    builder.UseFudieAuthorization();
});
```

`UseFudieAuthorization()` performs:

1. Discovers `IAggregateDescription` implementations across loaded assemblies
2. Registers all feature endpoints in the `ICatalogRegistry`
3. Adds `FudieAuthorizationMiddleware` to the pipeline
4. Maps the `GET /catalog` discovery endpoint

## Endpoint Security

```csharp
app.MapGet("/orders", GetOrders)
    .RequireAuthenticated()
    .RequireGroup("admin")
    .WithDescriptionCatalog("List all orders");

app.MapPost("/internal/sync", SyncData)
    .RequireInternal();

app.MapGet("/health", HealthCheck)
    .RequirePlatform();
```

### Authorization Requirements

| Requirement | Description |
|-------------|-------------|
| `RequireAuthenticated()` | Valid JWT token required |
| `RequirePlatform()` | Platform-level access only |
| `RequireInternal()` | Internal service-to-service calls only |
| `RequireGroup(name)` | User must belong to the specified group |
| `WithDescriptionCatalog(desc)` | Adds endpoint description to the catalog |

## Catalog Discovery

`GET /catalog` returns all registered endpoints with their metadata:

```json
{
  "service": "orders-api",
  "endpoints": [
    {
      "verb": "GET",
      "route": "/orders",
      "description": "List all orders",
      "aggregate": "orders",
      "scope": "orders:read"
    }
  ]
}
```

## Public Types

| Type | Description |
|------|-------------|
| `FudieUser` | Scoped service extracting user info from HTTP context claims |
| `FudieAuthorizationMiddleware` | Middleware enforcing authorization rules |
| `FudieAuthorizationExtensions` | `UseFudieAuthorization()` extension method |
| `CatalogRegistry` | Singleton storing registered endpoint entries |
| `CatalogEndpointExtensions` | Maps the `GET /catalog` endpoint |
| `EndpointAuthExtensions` | `RequireAuthenticated()`, `RequireGroup()`, etc. |
| `AuthenticatedRequirement` | Metadata marker for authentication |
| `PlatformRequirement` | Metadata marker for platform access |
| `InternalRequirement` | Metadata marker for internal access |
| `GroupRequirement` | Metadata marker for group-based access |

## Dependencies

- `Microsoft.AspNetCore.App` (framework reference)
- `Fudie.Http`
- `Fudie.Security`
- `Fudie.DependencyInjection`

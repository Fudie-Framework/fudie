# Fudie.OpenApi

Auto-discovers OpenAPI YAML contracts from the file system and configures SwaggerUI with development-mode serving and no-cache headers.

## Usage

```csharp
app.UseFudieOpenApi();
```

This scans the `OpenApi/` folder for YAML files and registers them with Swagger UI automatically.

## Behavior

- Discovers all `*.yaml` files under the `OpenApi/` directory
- Serves each file as a Swagger document
- Adds no-cache headers for development
- Configures SwaggerUI to display all discovered specs

## Public Types

| Type | Description |
|------|-------------|
| `FudieOpenApiExtensions` | Extension methods for OpenAPI discovery and SwaggerUI configuration |

## Dependencies

- `Microsoft.AspNetCore.App` (framework reference)
- `Swashbuckle.AspNetCore`

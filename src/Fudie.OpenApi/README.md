# Fudie.OpenApi

Auto-discovers OpenAPI YAML contracts from the file system and configures SwaggerUI with development-mode serving and no-cache headers.

## Quick Start

```csharp
app.UseFudieOpenApi();
```

Scans the `OpenApi/` folder for `*.yaml` files and registers them in Swagger UI. Only active in `Development` environment.

## Configuration

All settings are optional. Add them under `Fudie:OpenApi` in `appsettings.json`:

```json
{
    "Fudie": {
        "OpenApi": {
            "Folder": "OpenApi",
            "RequestPath": "OpenApi",
            "RoutePrefix": "swagger"
        }
    }
}
```

| Setting       | Default        | Description                                      |
|---------------|----------------|--------------------------------------------------|
| Folder        | `"OpenApi"`    | Physical directory where YAML files are located   |
| RequestPath   | same as Folder | URL path where YAML files are served              |
| RoutePrefix   | `"swagger"`    | URL path where Swagger UI is available            |

## Usage behind an API Gateway

When a microservice runs behind a reverse proxy (e.g., YARP) that routes by path prefix, the URLs need to include the service prefix so the gateway can forward the requests correctly.

Example: a **Plans** service routed through the gateway via `/plans/{**catch-all}`.

```json
{
    "Fudie": {
        "OpenApi": {
            "RequestPath": "plans/OpenApi",
            "RoutePrefix": "plans/swagger"
        }
    }
}
```

This produces:

| What                | URL                              |
|---------------------|----------------------------------|
| Swagger UI          | `/plans/swagger/index.html`      |
| YAML contracts      | `/plans/OpenApi/plan-api.yaml`   |
| Root redirect       | `/` → `/plans/swagger`           |

The physical files stay in `OpenApi/` (the default `Folder`). No need to move or rename anything.

### Why it works in all environments

- **Local development** (`http://localhost:7003`): the files are in `ContentRootPath/OpenApi/`, Swagger UI and YAML are served at the prefixed URLs. Everything resolves locally.
- **Docker via gateway** (`http://gateway:5176`): the gateway matches `/plans/{**catch-all}` and forwards to the Plans service. The service responds with Swagger UI and YAML at the same prefixed URLs.

## Cookie Authentication

By default, SwaggerUI is configured to send cookies with every request (`credentials: 'include'`). This ensures cookie-based authentication works seamlessly during development.

To disable this behavior:

```csharp
app.UseFudieOpenApi(withCredentials: false);
```

## Dependencies

- `Swashbuckle.AspNetCore` (>= 6.4.0)
- `Microsoft.AspNetCore.App` (framework reference)

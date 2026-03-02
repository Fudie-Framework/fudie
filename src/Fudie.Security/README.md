# Fudie.Security

Security infrastructure for Fudie microservices. Split into four packages with clear responsibilities.

## Packages

| Package | Purpose |
|---|---|
| **Fudie.Security** | Core contracts (`IPasswordHasher`, `IApiKeyGenerator`, `IFudieUser`, `ITokenGenerator`, `IJwtValidator`) and implementations (`BcryptPasswordHasher`, `ApiKeyGenerator`) |
| **Fudie.Security.Jwt** | JWT token generation and validation (`JwtValidator`, `ISigningKeyProvider`) |
| **Fudie.Security.Extensions** | JWKS key fetching with in-memory caching |
| **Fudie.Security.Http** | Authorization middleware, endpoint catalog, and security requirements for HTTP APIs |

## Dependency Graph

```
Fudie.Security.Http
  -> Fudie.Security
  -> Fudie.Http
  -> Fudie.DependencyInjection

Fudie.Security.Extensions
  -> Fudie.Security.Jwt
       -> Fudie.Security
       -> Fudie.DependencyInjection
```

## Installation

Reference the packages your service needs:

- **All HTTP services** need `Fudie.Security.Http` + `Fudie.Security.Extensions`
- **Auth service** (token issuer) needs `Fudie.Security` + `Fudie.Security.Jwt` directly

## Configuration

### 1. appsettings.json

```json
{
  "Fudie": {
    "Security": {
      "JwksUrl": "http://auth-service:8080/auth/jwks",
      "CacheRefreshMinutes": 60
    }
  }
}
```

| Key | Required | Default | Description |
|---|---|---|---|
| `JwksUrl` | Yes | - | Full URL of the JWKS endpoint that serves public signing keys |
| `CacheRefreshMinutes` | No | `60` | Minutes to cache JWKS keys before refreshing |

### 2. Program.cs

```csharp
// --- Service Registration ---

// Registers JWKS key provider, JWT validator, and in-memory cache.
// Reads from the Fudie:Security section in appsettings.json.
// Throws InvalidOperationException if the section or JwksUrl is missing.
builder.Services.AddFudieJwksProvider();

// --- Middleware Pipeline ---

// Enables authorization middleware, registers endpoints in the catalog,
// and maps the GET /catalog discovery endpoint.
featureBuilder.UseFudieAuthorization();
```

### 3. What each call registers

**`AddFudieJwksProvider()`** (from `Fudie.Security.Extensions`):
- Binds `FudieSecurityOptions` from configuration
- Registers `IMemoryCache`
- Registers HTTP client for JWKS endpoint
- Registers `ISigningKeyProvider` (singleton)
- Registers `IJwtValidator` (singleton, if not already registered)

**`UseFudieAuthorization()`** (from `Fudie.Security.Http`):
- Discovers `IAggregateDescription` implementations across loaded assemblies
- Registers all feature endpoints in the `ICatalogRegistry`
- Adds `FudieAuthorizationMiddleware` to the pipeline
- Maps the `GET /catalog` discovery endpoint

## Error Handling

The application will fail fast at startup if:

- The `Fudie:Security` configuration section is missing
- `JwksUrl` is empty or not provided

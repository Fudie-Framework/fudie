# Fudie.Security.Extensions

JWKS-based signing key provider with in-memory caching for JWT validation in distributed identity scenarios.

## Usage

```csharp
builder.Services.AddFudieJwksProvider();
```

This reads configuration from `Fudie:Security` in appsettings.json and registers:

| Service | Lifetime | Description |
|---------|----------|-------------|
| `FudieSecurityOptions` | Singleton | Bound configuration options |
| `IMemoryCache` | Singleton | In-memory cache for JWKS keys |
| `IJwksApi` | Singleton | HTTP client for JWKS endpoint (via Refit) |
| `ISigningKeyProvider` | Singleton | Fetches and caches signing keys |
| `IJwtValidator` | Singleton | JWT token validator (if not already registered) |

## Configuration

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
|-----|----------|---------|-------------|
| `JwksUrl` | Yes | - | Full URL of the JWKS endpoint |
| `CacheRefreshMinutes` | No | `60` | Minutes to cache keys before refreshing |

## Public Types

| Type | Description |
|------|-------------|
| `FudieSecurityOptions` | Configuration options (JwksUrl, CacheRefreshMinutes) |
| `JwkEntry` | Record representing a single JWK key |
| `JwksResponse` | Record representing the JWKS endpoint response |
| `ServiceCollectionExtensions` | `AddFudieJwksProvider()` extension method |

## Dependencies

- `Microsoft.Extensions.Caching.Memory`
- `Microsoft.Extensions.Http`
- `Refit` / `Refit.HttpClientFactory`
- `Fudie.Security.Jwt`

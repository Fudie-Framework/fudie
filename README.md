# Fudie

Modular .NET 8 framework for building microservices. Each package is independent and composable — reference only what you need.

[![CI](https://github.com/Fudie-Framework/fudie/actions/workflows/ci.yml/badge.svg)](https://github.com/Fudie-Framework/fudie/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Fudie-Framework/fudie/branch/main/graph/badge.svg)](https://codecov.io/gh/Fudie-Framework/fudie)

## Packages

### Core

| Package | Description |
|---|---|
| [Fudie.Domain](src/Fudie.Domain/) | DDD building blocks: entities, aggregate roots, domain events, result pattern, and base command types. Zero dependencies. |
| [Fudie.Validation](src/Fudie.Validation/) | Guard patterns (`NotFoundGuard`, `ConflictGuard`, `UnauthorizedGuard`, `ValidationGuard`) and result unwrapping extensions. |
| [Fudie.Infrastructure](src/Fudie.Infrastructure/) | EF Core abstractions: repository interfaces (`IGet`, `IAdd`, `IUpdate`, `IRemove`), `FudieDbContext` base class, unit of work, and change tracking. |
| [Fudie.Generator](src/Fudie.Generator/) | Source generator that creates repository implementations from interface definitions. Supports query methods, eager loading, tracking, and split queries. |

### HTTP

| Package | Description |
|---|---|
| [Fudie.Http](src/Fudie.Http/) | ASP.NET Core infrastructure: global exception handler (maps domain exceptions to RFC 7807), feature module discovery, and pipeline configuration. |
| [Fudie.OpenApi](src/Fudie.OpenApi/) | Auto-discovers OpenAPI YAML contracts and configures SwaggerUI. |

### Security

| Package | Description |
|---|---|
| [Fudie.Security](src/Fudie.Security/) | Core security contracts (`IPasswordHasher`, `IApiKeyGenerator`, `IFudieUser`, `ITokenGenerator`, `IJwtValidator`) and implementations (`BcryptPasswordHasher`, `ApiKeyGenerator`). |
| [Fudie.Security.Jwt](src/Fudie.Security.Jwt/) | JWT token generation and validation using ECDSA keys. |
| [Fudie.Security.Extensions](src/Fudie.Security.Extensions/) | JWKS key fetching with in-memory caching for distributed identity scenarios. |
| [Fudie.Security.Http](src/Fudie.Security.Http/) | Authorization middleware, endpoint catalog discovery, and security requirements for HTTP APIs. |

### Infrastructure

| Package | Description |
|---|---|
| [Fudie.DependencyInjection](src/Fudie.DependencyInjection/) | Attribute-based automatic DI registration via assembly scanning. |
| [Fudie.PubSub](src/Fudie.PubSub/) | Messaging core: transport contracts, envelope pattern, scoped message context, and hosted message consumer. Provider-agnostic. |

## Dependency Graph

```
Fudie.Domain (zero deps)
├── Fudie.Validation (+ FluentValidation)
├── Fudie.Infrastructure (+ EF Core)
│   └── Fudie.Generator (source gen, netstandard2.0)
└── Fudie.Http (+ FluentValidation)

Fudie.DependencyInjection (standalone)

Fudie.OpenApi (standalone, Swashbuckle)

Fudie.PubSub (standalone, DI abstractions)

Fudie.Security (standalone)
├── Fudie.Security.Jwt (+ IdentityModel)
│   └── Fudie.Security.Extensions (+ Refit, caching)
└── Fudie.Security.Http (+ Http, DI)
```

## Getting Started

```bash
# Build
dotnet build

# Test
dotnet test
```

## License

[MIT](LICENSE)

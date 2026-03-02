# Fudie.Security.Jwt

JWT token generation and validation using ECDSA keys for session management and claims extraction.

## Features

- Token generation with ECDSA signing
- Token validation against multiple signing key providers
- Claims extraction into `FudieTokenContext`
- JWKS public key exposure for federated validation

## Public Types

| Type | Description |
|------|-------------|
| `ISigningKeyProvider` | Interface for retrieving signing keys |
| `JwtKeyProvider` | Loads ECDSA private key from config, exposes public JWKS |
| `JwtValidator` | Validates JWT tokens using all registered `ISigningKeyProvider` instances |
| `TokenGenerator` | Generates signed JWT session tokens with claims |

### Token Claims

Generated tokens include:

| Claim | Description |
|-------|-------------|
| `sub` | User ID |
| `tid` | Tenant ID |
| `owner` | Whether the user is an owner |
| `groups` | User groups |
| `add` | Additional scopes |
| `exc` | Excluded scopes |

## Dependencies

- `Microsoft.Extensions.Configuration.Abstractions`
- `Microsoft.IdentityModel.JsonWebTokens`
- `Microsoft.IdentityModel.Tokens`
- `Fudie.DependencyInjection`
- `Fudie.Security`

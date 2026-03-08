# Fudie.Http

HTTP/ASP.NET Core infrastructure for exception handling, feature endpoint mapping, and custom problem details responses.

## Features

### Global Exception Handler

Maps domain exceptions to RFC 7807 problem details responses. Includes `errorCode` in the response when available for traceability:

| Exception | HTTP Status |
|-----------|-------------|
| `UnauthorizedException` | 401 Unauthorized |
| `KeyNotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| `ValidationException` | 422 Unprocessable Entity |
| `ArgumentException` | 400 Bad Request |
| Other | 500 Internal Server Error |

### Feature Builder

Context builder for configuring the feature pipeline after endpoints are mapped:

```csharp
app.MapFudieFeatures(builder =>
{
    builder.UseFudieAuthorization();
});
```

### Custom Problem Details

RFC 7807 compliant error DTO:

```json
{
  "title": "Validation Error",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "extensions": {
    "traceId": "...",
    "errors": {
      "Name": [{ "code": "Customer.Name.Required", "message": "Name is required" }]
    }
  }
}
```

For `ConflictException` and `UnauthorizedException` with an `ErrorCode`:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "Slug already taken",
  "extensions": {
    "traceId": "...",
    "errorCode": "Customer.Slug.AlreadyExists"
  }
}
```

## Public Types

| Type | Description |
|------|-------------|
| `CustomProblemDetails` | RFC 7807 problem details DTO |
| `GlobalExceptionHandler` | `IExceptionHandler` mapping domain exceptions to HTTP status codes |
| `FeatureBuilder` | Pipeline configuration context after endpoint mapping |
| `FeatureEndpointMapping` | Record mapping feature class name, namespace, and endpoint |
| `IFeatureModule` | Interface for endpoint registration |
| `RouteExtension` | Extension methods for discovering and mapping features |

## Dependencies

- `Microsoft.AspNetCore.App` (framework reference)
- `FluentValidation`
- `Fudie.Domain`

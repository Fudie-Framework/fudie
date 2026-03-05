namespace Fudie.Security.Http;

/// <summary>
/// Middleware that enforces Fudie authorization rules based on endpoint metadata
/// and JWT token validation.
/// </summary>
public class FudieAuthorizationMiddleware(RequestDelegate next)
{
    private const string InternalKeyHeader = "X-Internal-Key";

    /// <summary>
    /// Evaluates authorization requirements for the current endpoint and either
    /// allows the request to proceed or returns an appropriate problem response.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        IJwtValidator jwtValidator,
        ICatalogRegistry catalogRegistry,
        IConfiguration configuration)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await next(context);
            return;
        }

        var metadata = endpoint.Metadata;

        if (metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await TrySetTokenContext(context, jwtValidator);
            await next(context);
            return;
        }

        if (metadata.GetMetadata<InternalRequirement>() is not null)
        {
            await HandleInternalRequest(context, configuration, jwtValidator);
            return;
        }

        await HandleAuthenticatedRequest(context, metadata, endpoint, jwtValidator, catalogRegistry, configuration);
    }

    private async Task HandleInternalRequest(
        HttpContext context, IConfiguration configuration, IJwtValidator jwtValidator)
    {
        var internalSecret = configuration["Fudie:InternalSecret"];
        var incomingKey = context.Request.Headers[InternalKeyHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(incomingKey) || incomingKey != internalSecret)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "Invalid internal key",
                "https://tools.ietf.org/html/rfc7235#section-3.1");
            return;
        }

        await TrySetTokenContext(context, jwtValidator);
        await next(context);
    }

    private async Task HandleAuthenticatedRequest(
        HttpContext context, EndpointMetadataCollection metadata, Endpoint endpoint,
        IJwtValidator jwtValidator, ICatalogRegistry catalogRegistry, IConfiguration configuration)
    {
        var tokenContext = await ValidateJwt(context, jwtValidator);
        if (tokenContext is null)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized,
                "Unauthorized", "Authentication required",
                "https://tools.ietf.org/html/rfc7235#section-3.1");
            return;
        }

        var denied = EvaluateAccess(metadata, endpoint, tokenContext, catalogRegistry, configuration);
        if (denied is not null)
        {
            await WriteProblem(context, denied.Value.StatusCode,
                denied.Value.Title, denied.Value.Detail, denied.Value.Type);
            return;
        }

        SetTokenContext(context, tokenContext);
        await next(context);
    }

    private static (int StatusCode, string Title, string Detail, string Type)? EvaluateAccess(
        EndpointMetadataCollection metadata, Endpoint endpoint,
        FudieTokenContext tokenContext, ICatalogRegistry catalogRegistry, IConfiguration configuration)
    {
        var platform = CheckPlatformAccess(metadata, tokenContext, configuration);
        if (platform is not null) return platform;

        if (tokenContext.IsOwner) return null;

        if (metadata.GetMetadata<AuthenticatedRequirement>() is not null)
            return null;

        var className = catalogRegistry.FindClassName(endpoint);

        if (IsExcludedScope(className, tokenContext))
            return (StatusCodes.Status403Forbidden, "Forbidden",
                "Access to this endpoint has been revoked", "https://tools.ietf.org/html/rfc7231#section-6.5.3");

        if (IsAdditionalScope(className, tokenContext))
            return null;

        return CheckScopeAccess(catalogRegistry, endpoint, tokenContext);
    }

    private static (int StatusCode, string Title, string Detail, string Type)? CheckPlatformAccess(
        EndpointMetadataCollection metadata, FudieTokenContext tokenContext, IConfiguration configuration)
    {
        if (metadata.GetMetadata<PlatformRequirement>() is null)
            return null;

        var platformTenantId = configuration["Fudie:PlatformTenantId"];
        if (platformTenantId is null || tokenContext.TenantId?.ToString() != platformTenantId)
            return (StatusCodes.Status403Forbidden, "Forbidden",
                "Platform access required", "https://tools.ietf.org/html/rfc7231#section-6.5.3");

        return null;
    }

    private static bool IsExcludedScope(string? className, FudieTokenContext tokenContext)
        => className is not null && tokenContext.ExcludedScopes.Contains(className);

    private static bool IsAdditionalScope(string? className, FudieTokenContext tokenContext)
        => className is not null && tokenContext.AdditionalScopes.Contains(className);

    private static (int StatusCode, string Title, string Detail, string Type)? CheckScopeAccess(
        ICatalogRegistry catalogRegistry, Endpoint endpoint, FudieTokenContext tokenContext)
    {
        var scope = catalogRegistry.FindScope(endpoint);

        if (scope is not null && tokenContext.Groups.Contains(scope))
            return null;

        return (StatusCodes.Status403Forbidden, "Forbidden",
            "Insufficient permissions", "https://tools.ietf.org/html/rfc7231#section-6.5.3");
    }

    private static async Task<FudieTokenContext?> ValidateJwt(HttpContext context, IJwtValidator jwtValidator)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return await jwtValidator.ValidateTokenAsync(authHeader["Bearer ".Length..]);
    }

    private static async Task TrySetTokenContext(HttpContext context, IJwtValidator jwtValidator)
    {
        var tokenContext = await ValidateJwt(context, jwtValidator);
        SetTokenContext(context, tokenContext);
    }

    private static void SetTokenContext(HttpContext context, FudieTokenContext? tokenContext)
    {
        if (tokenContext is null) return;

        context.Items["FudieTokenContext"] = tokenContext;

        var claims = new List<Claim>
        {
            new("sub", tokenContext.UserId.ToString())
        };

        if (tokenContext.TenantId is not null)
            claims.Add(new("tid", tokenContext.TenantId.Value.ToString()));

        if (tokenContext.IsOwner)
            claims.Add(new("owner", "true"));

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "FudieToken"));
    }

    private static async Task WriteProblem(
        HttpContext context, int statusCode, string title, string detail, string type)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new CustomProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = context.Request.Path,
            Extensions = new Dictionary<string, object>
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}

namespace Fudie.Security.Http;

/// <summary>
/// Scoped implementation of <see cref="IFudieUser"/> that extracts user information
/// from the current HTTP context claims principal.
/// </summary>
[Injectable]
public class FudieUser(IHttpContextAccessor httpContextAccessor) : IFudieUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public Guid? UserId =>
        User?.FindFirst("sub")?.Value is { } sub && Guid.TryParse(sub, out var id) ? id : null;

    /// <inheritdoc />
    public Guid? TenantId =>
        User?.FindFirst("tid")?.Value is { } tid && Guid.TryParse(tid, out var id) ? id : null;

    /// <inheritdoc />
    public bool IsOwner => User?.HasClaim("owner", "true") ?? false;
}

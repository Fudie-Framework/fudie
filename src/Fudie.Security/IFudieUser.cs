namespace Fudie.Security;

/// <summary>
/// Represents the authenticated user in the current HTTP request scope.
/// </summary>
public interface IFudieUser
{
    /// <summary>
    /// Authenticated user identifier, or <c>null</c> if not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Tenant identifier, or <c>null</c> if not associated with a tenant.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Whether the user is the tenant owner.
    /// </summary>
    bool IsOwner { get; }

    /// <summary>
    /// Whether the user has been successfully authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Session identifier for user tokens, or <c>null</c> for app tokens.
    /// </summary>
    Guid? SessionId { get; }

    /// <summary>
    /// External application identifier for API key tokens, or <c>null</c> for user tokens.
    /// </summary>
    Guid? AppId { get; }
}

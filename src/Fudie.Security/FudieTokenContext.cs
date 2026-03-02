namespace Fudie.Security;

/// <summary>
/// Immutable security context extracted from a validated JWT token.
/// </summary>
/// <param name="UserId">Authenticated user identifier.</param>
/// <param name="TenantId">Tenant identifier, or <c>null</c> if not present in the token.</param>
/// <param name="IsOwner">Whether the user is the tenant owner.</param>
/// <param name="Groups">Permission groups assigned to the user.</param>
/// <param name="AdditionalScopes">Extra scopes granted beyond group defaults.</param>
/// <param name="ExcludedScopes">Scopes explicitly revoked from the user.</param>
public record FudieTokenContext(
    Guid UserId,
    Guid? TenantId,
    bool IsOwner,
    string[] Groups,
    string[] AdditionalScopes,
    string[] ExcludedScopes);

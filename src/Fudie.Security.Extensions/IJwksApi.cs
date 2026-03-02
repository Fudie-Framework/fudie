namespace Fudie.Security.Extensions;

/// <summary>
/// HTTP client for retrieving JSON Web Key Sets.
/// The full URL is resolved from <see cref="FudieSecurityOptions.JwksUrl"/> via HttpClient.BaseAddress.
/// </summary>
internal interface IJwksApi
{
    /// <summary>
    /// Retrieves the JWKS containing public signing keys.
    /// </summary>
    [Get("")]
    Task<JwksResponse> GetJwksAsync();
}

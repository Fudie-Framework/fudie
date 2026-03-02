namespace Fudie.Security.Extensions;

/// <summary>
/// Configuration options for Fudie JWKS key provider.
/// </summary>
public class FudieSecurityOptions
{
    /// <summary>
    /// Configuration section name used for binding from appsettings.
    /// </summary>
    public const string SectionName = "Fudie:Security";

    /// <summary>
    /// Full URL of the JWKS endpoint used to retrieve public signing keys.
    /// </summary>
    public string JwksUrl { get; set; } = string.Empty;

    /// <summary>
    /// Duration in minutes to cache JWKS keys before refreshing. Defaults to 60.
    /// </summary>
    public int CacheRefreshMinutes { get; set; } = 60;
}

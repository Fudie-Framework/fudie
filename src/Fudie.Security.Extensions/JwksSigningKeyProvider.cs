namespace Fudie.Security.Extensions;

/// <summary>
/// Provides ECDSA signing keys by fetching JWKS and caching them with <see cref="IMemoryCache"/>.
/// </summary>
public class JwksSigningKeyProvider : SigningKeyProviderBase
{
    private const string CacheKey = "FudieSecurity:SigningKeys";

    private readonly IJwksApi _jwksApi;
    private readonly IMemoryCache _cache;
    private readonly IOptions<FudieSecurityOptions> _options;

    internal JwksSigningKeyProvider(
        IJwksApi jwksApi,
        IMemoryCache cache,
        IOptions<FudieSecurityOptions> options)
    {
        _jwksApi = jwksApi;
        _cache = cache;
        _options = options;
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        var keys = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(_options.Value.CacheRefreshMinutes);

            var jwks = await _jwksApi.GetJwksAsync();
            return jwks.Keys.Select(k => CreateKey(k.X, k.Y, k.Kid, k.Crv)).ToArray();
        });

        return keys ?? [];
    }
}

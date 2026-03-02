namespace Fudie.Security.Extensions;

/// <summary>
/// Provides ECDSA signing keys by fetching JWKS and caching them with <see cref="IMemoryCache"/>.
/// </summary>
internal class JwksSigningKeyProvider(
    IJwksApi jwksApi,
    IMemoryCache cache,
    IOptions<FudieSecurityOptions> options) : ISigningKeyProvider
{
    private const string CacheKey = "FudieSecurity:SigningKeys";

    /// <inheritdoc />
    public async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        var keys = await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(options.Value.CacheRefreshMinutes);

            var jwks = await jwksApi.GetJwksAsync();
            return jwks.Keys.Select(CreateKey).ToArray();
        });

        return keys ?? [];
    }

    private static ECDsaSecurityKey CreateKey(JwkEntry jwk)
    {
        var ecdsa = ECDsa.Create(new ECParameters
        {
            Curve = GetCurve(jwk.Crv),
            Q = new ECPoint
            {
                X = Base64UrlEncoder.DecodeBytes(jwk.X),
                Y = Base64UrlEncoder.DecodeBytes(jwk.Y)
            }
        });

        return new ECDsaSecurityKey(ecdsa) { KeyId = jwk.Kid };
    }

    internal static ECCurve GetCurve(string crv) => crv switch
    {
        "P-256" => ECCurve.NamedCurves.nistP256,
        "P-384" => ECCurve.NamedCurves.nistP384,
        "P-521" => ECCurve.NamedCurves.nistP521,
        _ => throw new NotSupportedException($"Unsupported curve: {crv}")
    };
}

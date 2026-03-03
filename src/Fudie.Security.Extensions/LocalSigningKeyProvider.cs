namespace Fudie.Security.Extensions;

/// <summary>
/// Provides ECDSA signing keys from the local <see cref="IJwtKeyProvider"/> without HTTP calls.
/// </summary>
public class LocalSigningKeyProvider(IJwtKeyProvider jwtKeyProvider) : SigningKeyProviderBase
{
    /// <inheritdoc />
    public override Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
    {
        var jwk = jwtKeyProvider.GetJsonWebKey();
        SecurityKey key = CreateKey(jwk.X, jwk.Y, jwk.Kid, jwk.Crv);
        return Task.FromResult<IEnumerable<SecurityKey>>([key]);
    }
}

namespace Fudie.Security.Extensions;

/// <summary>
/// Base class that provides shared ECDSA key-creation logic for signing key providers.
/// </summary>
public abstract class SigningKeyProviderBase : ISigningKeyProvider
{
    public abstract Task<IEnumerable<SecurityKey>> GetSigningKeysAsync();

    protected static ECDsaSecurityKey CreateKey(string x, string y, string kid, string crv)
    {
        var ecdsa = ECDsa.Create(new ECParameters
        {
            Curve = GetCurve(crv),
            Q = new ECPoint
            {
                X = Base64UrlEncoder.DecodeBytes(x),
                Y = Base64UrlEncoder.DecodeBytes(y)
            }
        });

        return new ECDsaSecurityKey(ecdsa) { KeyId = kid };
    }

    protected static ECCurve GetCurve(string crv) => crv switch
    {
        "P-256" => ECCurve.NamedCurves.nistP256,
        "P-384" => ECCurve.NamedCurves.nistP384,
        "P-521" => ECCurve.NamedCurves.nistP521,
        _ => throw new NotSupportedException($"Unsupported curve: {crv}")
    };
}

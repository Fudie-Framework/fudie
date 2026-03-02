namespace Fudie.Security;

/// <summary>
/// Provides the cryptographic keys used for JWT signing and public key distribution.
/// </summary>
public interface IJwtKeyProvider
{
    /// <summary>
    /// Returns the ECDSA private key used to sign JWT tokens.
    /// </summary>
    /// <returns>The <see cref="ECDsa"/> private key instance.</returns>
    ECDsa GetPrivateKey();

    /// <summary>
    /// Returns the public key in JWK format, suitable for JWKS endpoint exposure.
    /// </summary>
    /// <returns>The <see cref="JsonWebKey"/> containing the public key and key identifier.</returns>
    JsonWebKey GetJsonWebKey();
}

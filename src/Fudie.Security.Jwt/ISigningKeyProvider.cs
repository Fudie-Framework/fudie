namespace Fudie.Security.Jwt;

/// <summary>
/// Provides signing keys for JWT validation.
/// Implement this interface to integrate with different key sources (JWKS, PEM, symmetric, etc.).
/// </summary>
public interface ISigningKeyProvider
{
    /// <summary>
    /// Retrieves the signing keys used to validate JWT tokens.
    /// </summary>
    /// <returns>A collection of signing keys. May return multiple keys for key rotation scenarios.</returns>
    Task<IEnumerable<SecurityKey>> GetSigningKeysAsync();
}

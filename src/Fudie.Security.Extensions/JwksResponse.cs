namespace Fudie.Security.Extensions;

/// <summary>
/// Represents a single JSON Web Key entry from a JWKS endpoint.
/// </summary>
/// <param name="Kty">Key type (e.g. "EC").</param>
/// <param name="Crv">Elliptic curve name (e.g. "P-256", "P-384", "P-521").</param>
/// <param name="X">Base64url-encoded X coordinate of the public key.</param>
/// <param name="Y">Base64url-encoded Y coordinate of the public key.</param>
/// <param name="Kid">Key identifier.</param>
/// <param name="Use">Intended use of the key (e.g. "sig").</param>
/// <param name="Alg">Algorithm intended for use with the key (e.g. "ES256").</param>
public record JwkEntry(string Kty, string Crv, string X, string Y, string Kid, string Use, string Alg);

/// <summary>
/// Response from a JWKS endpoint containing an array of public keys.
/// </summary>
/// <param name="Keys">Array of JSON Web Key entries.</param>
public record JwksResponse(JwkEntry[] Keys);

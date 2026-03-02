namespace Fudie.Security;

/// <summary>
/// Result of generating a new API key.
/// </summary>
/// <param name="RawKey">The raw API key to return to the caller (shown once).</param>
/// <param name="Hash">The BCrypt hash of the API key for storage.</param>
/// <param name="Salt">The BCrypt salt used for hashing.</param>
/// <param name="Prefix">The first 8 characters of the API key for identification.</param>
public record ApiKeyResult(string RawKey, string Hash, string Salt, string Prefix);

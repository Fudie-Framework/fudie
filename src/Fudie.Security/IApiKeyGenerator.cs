namespace Fudie.Security;

/// <summary>
/// Generates and verifies API keys using a secure hashing algorithm.
/// </summary>
public interface IApiKeyGenerator
{
    /// <summary>
    /// Generates a new API key with its hash, salt, and prefix.
    /// </summary>
    /// <returns>An <see cref="ApiKeyResult"/> containing the raw key and its hashed components.</returns>
    ApiKeyResult Generate();

    /// <summary>
    /// Verifies a raw API key against a stored hash and salt.
    /// </summary>
    /// <param name="rawKey">The raw API key to verify.</param>
    /// <param name="storedHash">The stored BCrypt hash.</param>
    /// <param name="storedSalt">The stored BCrypt salt.</param>
    /// <returns><c>true</c> if the key matches; otherwise, <c>false</c>.</returns>
    bool Verify(string rawKey, string storedHash, string storedSalt);
}

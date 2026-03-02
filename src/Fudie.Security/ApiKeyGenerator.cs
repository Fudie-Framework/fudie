namespace Fudie.Security;

/// <summary>
/// BCrypt-based implementation of <see cref="IApiKeyGenerator"/>.
/// Generates secure random API keys and verifies them using <see cref="IPasswordHasher"/>.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class ApiKeyGenerator(IPasswordHasher passwordHasher) : IApiKeyGenerator
{
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public ApiKeyResult Generate()
    {
        var rawKey = $"fud_{RandomNumberGenerator.GetString(AllowedChars, 32)}";
        var salt = passwordHasher.GenerateSalt();
        var hash = passwordHasher.Hash(rawKey, salt);
        var prefix = rawKey[..8];
        return new ApiKeyResult(rawKey, hash, salt, prefix);
    }

    public bool Verify(string rawKey, string storedHash, string storedSalt)
    {
        return passwordHasher.Verify(rawKey, storedHash, storedSalt);
    }
}

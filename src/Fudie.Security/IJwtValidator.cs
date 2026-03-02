namespace Fudie.Security;

/// <summary>
/// Validates JWT tokens and extracts the Fudie security context.
/// </summary>
public interface IJwtValidator
{
    /// <summary>
    /// Validates a JWT token and extracts the security context.
    /// </summary>
    /// <param name="token">The raw JWT token string.</param>
    /// <returns>The extracted <see cref="FudieTokenContext"/> if valid; <c>null</c> if validation fails.</returns>
    Task<FudieTokenContext?> ValidateTokenAsync(string token);
}

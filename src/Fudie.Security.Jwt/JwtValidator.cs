namespace Fudie.Security.Jwt;

/// <summary>
/// Validates JWT tokens using signing keys collected from all registered <see cref="ISigningKeyProvider"/> instances.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class JwtValidator(IEnumerable<ISigningKeyProvider> keyProviders) : IJwtValidator
{
    /// <inheritdoc />
    public async Task<FudieTokenContext?> ValidateTokenAsync(string token)
    {
        var keys = new List<SecurityKey>();
        foreach (var provider in keyProviders)
        {
            var providerKeys = await provider.GetSigningKeysAsync();
            keys.AddRange(providerKeys);
        }

        if (keys.Count == 0) return null;

        var handler = new JsonWebTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ClockSkew = TimeSpan.FromSeconds(10)
        };

        var result = await handler.ValidateTokenAsync(token, validationParameters);
        if (!result.IsValid) return null;

        return ExtractContext(result);
    }

    private static FudieTokenContext ExtractContext(TokenValidationResult result)
    {
        var claims = result.Claims;

        return new FudieTokenContext(
            ExtractGuid(claims, "sub") ?? Guid.Empty,
            ExtractGuid(claims, "tid"),
            claims.TryGetValue("owner", out var owner) && owner is true,
            ExtractStringArray(claims, "groups"),
            ExtractStringArray(claims, "add"),
            ExtractStringArray(claims, "exc"));
    }

    private static Guid? ExtractGuid(IDictionary<string, object> claims, string key)
    {
        return claims.TryGetValue(key, out var value) && Guid.TryParse(value.ToString(), out var guid)
            ? guid
            : null;
    }

    private static string[] ExtractStringArray(IDictionary<string, object> claims, string key)
    {
        if (!claims.TryGetValue(key, out var value)) return [];

        return value switch
        {
            string s => [s],
            List<object> list => list.Select(x => x.ToString()!).ToArray(),
            _ => []
        };
    }
}

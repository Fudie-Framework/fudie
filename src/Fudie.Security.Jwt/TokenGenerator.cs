namespace Fudie.Security.Jwt;

/// <summary>
/// Generates signed JWT tokens containing session claims using ECDSA keys.
/// </summary>
/// <param name="jwtKeyProvider">The provider for signing keys.</param>
/// <param name="configuration">The application configuration for token lifetime settings.</param>
[Injectable(ServiceLifetime.Singleton)]
public class TokenGenerator(
    IJwtKeyProvider jwtKeyProvider,
    IConfiguration configuration) : ITokenGenerator
{
    private readonly TimeSpan _sessionTokenLifetime = TimeSpan.FromSeconds(
        int.TryParse(configuration["Fudie:SessionTokenLifetimeSeconds"], out var seconds)
            ? seconds
            : 180);

    /// <inheritdoc />
    public string GenerateUserToken(FudieTokenContext data, Guid sessionId)
    {
        var claims = BuildBaseClaims(data);
        claims["sid"] = sessionId.ToString();
        return CreateToken(claims, _sessionTokenLifetime);
    }

    /// <inheritdoc />
    public string GenerateAppToken(FudieTokenContext data, Guid appId)
    {
        var claims = BuildBaseClaims(data);
        claims["app"] = appId.ToString();
        return CreateToken(claims, _sessionTokenLifetime);
    }

    private static Dictionary<string, object> BuildBaseClaims(FudieTokenContext data)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = data.UserId.ToString()
        };

        if (data.TenantId is not null)
        {
            claims["tid"] = data.TenantId.Value.ToString();

            if (data.IsOwner)
            {
                claims["owner"] = true;
            }
            else
            {
                claims["groups"] = data.Groups;
                claims["add"] = data.AdditionalScopes;
                claims["exc"] = data.ExcludedScopes;
            }
        }

        return claims;
    }

    private string CreateToken(Dictionary<string, object> claims, TimeSpan lifetime)
    {
        var key = new ECDsaSecurityKey(jwtKeyProvider.GetPrivateKey())
        {
            KeyId = jwtKeyProvider.GetJsonWebKey().Kid
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.Add(lifetime),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256)
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}

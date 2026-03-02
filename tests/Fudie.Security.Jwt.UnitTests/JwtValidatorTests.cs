namespace Fudie.Security.Jwt.UnitTests;

public class JwtValidatorTests
{
    private static ECDsa CreateEcKey() => ECDsa.Create(ECCurve.NamedCurves.nistP256);

    private static string CreateToken(ECDsa key, Dictionary<string, object>? extraClaims = null, DateTime? expires = null)
    {
        var secKey = new ECDsaSecurityKey(key) { KeyId = "test-kid" };
        var credentials = new SigningCredentials(secKey, SecurityAlgorithms.EcdsaSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Expires = expires ?? DateTime.UtcNow.AddHours(1),
            Claims = extraClaims ?? new Dictionary<string, object>()
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    private static Mock<ISigningKeyProvider> CreateProviderMock(params SecurityKey[] keys)
    {
        var mock = new Mock<ISigningKeyProvider>();
        mock.Setup(p => p.GetSigningKeysAsync())
            .ReturnsAsync(keys);
        return mock;
    }

    private static (JwtValidator validator, ECDsa key) CreateValidatorWithKey()
    {
        var ecdsa = CreateEcKey();
        var secKey = new ECDsaSecurityKey(ecdsa) { KeyId = "test-kid" };
        var provider = CreateProviderMock(secKey);
        return (new JwtValidator([provider.Object]), ecdsa);
    }

    // --- No keys / empty providers ---

    [Fact]
    public async Task ValidateTokenAsync_no_providers_returns_null()
    {
        var validator = new JwtValidator([]);
        var result = await validator.ValidateTokenAsync("any.token.here");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_provider_returns_empty_keys_returns_null()
    {
        var provider = CreateProviderMock();
        var validator = new JwtValidator([provider.Object]);
        var result = await validator.ValidateTokenAsync("any.token.here");
        result.Should().BeNull();
    }

    // --- Invalid / expired / wrong key ---

    [Fact]
    public async Task ValidateTokenAsync_garbage_token_returns_null()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var result = await validator.ValidateTokenAsync("not-a-jwt");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_wrong_key_returns_null()
    {
        using var signingKey = CreateEcKey();
        using var wrongKey = CreateEcKey();
        var token = CreateToken(signingKey);

        var wrongSecKey = new ECDsaSecurityKey(wrongKey) { KeyId = "wrong-kid" };
        var provider = CreateProviderMock(wrongSecKey);
        var validator = new JwtValidator([provider.Object]);

        var result = await validator.ValidateTokenAsync(token);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_expired_token_returns_null()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, expires: DateTime.UtcNow.AddHours(-1));
        var result = await validator.ValidateTokenAsync(token);
        result.Should().BeNull();
    }

    // --- Valid token: full context ---

    [Fact]
    public async Task ValidateTokenAsync_valid_token_extracts_full_context()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var claims = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["tid"] = tenantId.ToString(),
            ["owner"] = true,
            ["groups"] = new[] { "admin", "editor" },
            ["add"] = new[] { "extra-scope" },
            ["exc"] = new[] { "blocked-scope" }
        };

        var token = CreateToken(key, claims);
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        result.IsOwner.Should().BeTrue();
        result.Groups.Should().BeEquivalentTo("admin", "editor");
        result.AdditionalScopes.Should().BeEquivalentTo("extra-scope");
        result.ExcludedScopes.Should().BeEquivalentTo("blocked-scope");
    }

    // --- Minimal / missing claims ---

    [Fact]
    public async Task ValidateTokenAsync_missing_optional_claims_uses_defaults()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key);
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(Guid.Empty);
        result.TenantId.Should().BeNull();
        result.IsOwner.Should().BeFalse();
        result.Groups.Should().BeEmpty();
        result.AdditionalScopes.Should().BeEmpty();
        result.ExcludedScopes.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateTokenAsync_invalid_sub_guid_returns_empty_guid()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["sub"] = "not-a-guid" });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task ValidateTokenAsync_invalid_tid_returns_null_tenant()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["tid"] = "not-a-guid" });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.TenantId.Should().BeNull();
    }

    // --- Single string claim for array fields ---

    [Fact]
    public async Task ValidateTokenAsync_single_string_group_extracts_as_array()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["groups"] = "solo-group" });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.Groups.Should().BeEquivalentTo("solo-group");
    }

    // --- Non-string/non-list claim for array field → default empty ---

    [Fact]
    public async Task ValidateTokenAsync_integer_in_array_field_returns_empty_array()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["groups"] = 42 });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.Groups.Should().BeEmpty();
    }

    // --- owner claim edge cases ---

    [Fact]
    public async Task ValidateTokenAsync_owner_non_boolean_returns_is_owner_false()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["owner"] = "yes" });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.IsOwner.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateTokenAsync_owner_false_returns_is_owner_false()
    {
        var (validator, key) = CreateValidatorWithKey();
        using var _ = key;
        var token = CreateToken(key, new Dictionary<string, object> { ["owner"] = false });
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.IsOwner.Should().BeFalse();
    }

    // --- Multi-provider ---

    [Fact]
    public async Task ValidateTokenAsync_multi_provider_aggregates_keys()
    {
        using var ecdsa = CreateEcKey();
        var userId = Guid.NewGuid();
        var token = CreateToken(ecdsa, new Dictionary<string, object> { ["sub"] = userId.ToString() });

        using var otherKey = CreateEcKey();
        var otherSecKey = new ECDsaSecurityKey(otherKey) { KeyId = "other-kid" };
        var correctSecKey = new ECDsaSecurityKey(ecdsa) { KeyId = "test-kid" };

        var provider1 = CreateProviderMock(otherSecKey);
        var provider2 = CreateProviderMock(correctSecKey);

        var validator = new JwtValidator([provider1.Object, provider2.Object]);
        var result = await validator.ValidateTokenAsync(token);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        provider1.Verify(p => p.GetSigningKeysAsync(), Times.Once);
        provider2.Verify(p => p.GetSigningKeysAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_all_providers_empty_returns_null()
    {
        var provider1 = CreateProviderMock();
        var provider2 = CreateProviderMock();
        var validator = new JwtValidator([provider1.Object, provider2.Object]);

        var result = await validator.ValidateTokenAsync("any.token.here");
        result.Should().BeNull();
    }

    // --- Contract ---

    [Fact]
    public void JwtValidator_implements_IJwtValidator()
    {
        typeof(JwtValidator).Should().Implement<IJwtValidator>();
    }

    [Fact]
    public void JwtValidator_is_public_class()
    {
        typeof(JwtValidator).IsPublic.Should().BeTrue();
        typeof(JwtValidator).IsClass.Should().BeTrue();
    }
}

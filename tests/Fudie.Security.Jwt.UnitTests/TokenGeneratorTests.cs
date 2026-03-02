namespace Fudie.Security.Jwt.UnitTests;

public class TokenGeneratorTests
{
    private readonly ECDsa _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly string _kid = "test-kid";
    private readonly Mock<IJwtKeyProvider> _keyProvider = new();
    private readonly TokenGenerator _sut;

    public TokenGeneratorTests()
    {
        _keyProvider.Setup(k => k.GetPrivateKey()).Returns(_ecdsa);
        _keyProvider.Setup(k => k.GetJsonWebKey()).Returns(new JsonWebKey { Kid = _kid });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { })
            .Build();

        _sut = new TokenGenerator(_keyProvider.Object, configuration);
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Sin tenant
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithoutTenant_ContainsSubClaim()
    {
        var userId = Guid.NewGuid();
        var data = new FudieTokenContext(userId, null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("sub").Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void SessionToken_WithoutTenant_DoesNotContainTidClaim()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("tid", out _).Should().BeFalse();
    }

    [Fact]
    public void SessionToken_WithoutTenant_DoesNotContainPermissionClaims()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("owner", out _).Should().BeFalse();
        jwt.TryGetClaim("groups", out _).Should().BeFalse();
        jwt.TryGetClaim("add", out _).Should().BeFalse();
        jwt.TryGetClaim("exc", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Owner
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithOwner_ContainsOwnerClaim()
    {
        var tenantId = Guid.NewGuid();
        var data = new FudieTokenContext(Guid.NewGuid(), tenantId, true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("owner").Value.Should().Be("true");
    }

    [Fact]
    public void SessionToken_WithOwner_ContainsTidClaim()
    {
        var tenantId = Guid.NewGuid();
        var data = new FudieTokenContext(Guid.NewGuid(), tenantId, true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.GetClaim("tid").Value.Should().Be(tenantId.ToString());
    }

    [Fact]
    public void SessionToken_WithOwner_DoesNotContainPermissionArrays()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), true, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("groups", out _).Should().BeFalse();
        jwt.TryGetClaim("add", out _).Should().BeFalse();
        jwt.TryGetClaim("exc", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Normal (con permisos)
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithPermissions_ContainsGroupsClaim()
    {
        var groups = new[] { "menu:read", "menu:write" };
        var data = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), false, groups, [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var groupsClaim = jwt.GetClaim("groups");
        groupsClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_ContainsAdditionalScopesClaim()
    {
        var add = new[] { "reservation-service:CancelReservation" };
        var data = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), false, [], add, []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var addClaim = jwt.GetClaim("add");
        addClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_ContainsExcludedScopesClaim()
    {
        var exc = new[] { "menu-service:SetMenuDepositPolicy" };
        var data = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), false, [], [], exc);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var excClaim = jwt.GetClaim("exc");
        excClaim.Should().NotBeNull();
    }

    [Fact]
    public void SessionToken_WithPermissions_DoesNotContainOwnerClaim()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), false, ["menu:read"], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.TryGetClaim("owner", out _).Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Lifetime configurado
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_WithConfiguredLifetime_UsesConfiguredValue()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var keyProvider = new Mock<IJwtKeyProvider>();
        keyProvider.Setup(k => k.GetPrivateKey()).Returns(ecdsa);
        keyProvider.Setup(k => k.GetJsonWebKey()).Returns(new JsonWebKey { Kid = "k" });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:SessionTokenLifetimeSeconds"] = "60"
            })
            .Build();

        var sut = new TokenGenerator(keyProvider.Object, config);
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        var exp = jwt.ValidTo;
        exp.Should().BeCloseTo(DateTime.UtcNow.AddSeconds(60), TimeSpan.FromSeconds(5));
    }

    // ──────────────────────────────────────────────
    // GenerateSessionToken — Firma y expiración
    // ──────────────────────────────────────────────

    [Fact]
    public void SessionToken_IsSignedWithES256()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Alg.Should().Be(SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public void SessionToken_HasCorrectKid()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        jwt.Kid.Should().Be(_kid);
    }

    [Fact]
    public async Task SessionToken_CanBeValidated()
    {
        var data = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        var token = _sut.GenerateSessionToken(data);

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new ECDsaSecurityKey(_ecdsa)
        });

        result.IsValid.Should().BeTrue();
    }
}

namespace Fudie.Security.Extensions.UnitTests;

public class SigningKeyProviderBaseTests
{
    private class TestableProvider : SigningKeyProviderBase
    {
        public override Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
            => Task.FromResult<IEnumerable<SecurityKey>>([]);

        public static ECDsaSecurityKey CallCreateKey(string x, string y, string kid, string crv)
            => CreateKey(x, y, kid, crv);

        public static ECCurve CallGetCurve(string crv)
            => GetCurve(crv);
    }

    private static (string X, string Y) GenerateKeyPair(ECCurve curve)
    {
        using var ecdsa = ECDsa.Create(curve);
        var p = ecdsa.ExportParameters(false);
        return (Base64UrlEncoder.Encode(p.Q.X!), Base64UrlEncoder.Encode(p.Q.Y!));
    }

    [Fact]
    public void CreateKey_ReturnsECDsaSecurityKeyWithCorrectKid()
    {
        var (x, y) = GenerateKeyPair(ECCurve.NamedCurves.nistP256);

        var key = TestableProvider.CallCreateKey(x, y, "my-kid", "P-256");

        key.Should().BeOfType<ECDsaSecurityKey>();
        key.KeyId.Should().Be("my-kid");
    }

    [Theory]
    [InlineData("P-256")]
    [InlineData("P-384")]
    [InlineData("P-521")]
    public void CreateKey_WithDifferentCurves_Succeeds(string crv)
    {
        var curve = crv switch
        {
            "P-256" => ECCurve.NamedCurves.nistP256,
            "P-384" => ECCurve.NamedCurves.nistP384,
            "P-521" => ECCurve.NamedCurves.nistP521,
            _ => throw new ArgumentException(crv)
        };
        var (x, y) = GenerateKeyPair(curve);

        var key = TestableProvider.CallCreateKey(x, y, "kid-" + crv, crv);

        key.KeyId.Should().Be("kid-" + crv);
    }

    [Theory]
    [InlineData("P-256")]
    [InlineData("P-384")]
    [InlineData("P-521")]
    public void GetCurve_WithSupportedCurve_ReturnsCurve(string crv)
    {
        var curve = TestableProvider.CallGetCurve(crv);

        curve.Oid.Should().NotBeNull();
    }

    [Fact]
    public void GetCurve_WithUnsupportedCurve_Throws()
    {
        var act = () => TestableProvider.CallGetCurve("P-999");

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported curve: P-999");
    }
}

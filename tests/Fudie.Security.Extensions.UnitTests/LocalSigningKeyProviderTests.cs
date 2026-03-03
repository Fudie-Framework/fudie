namespace Fudie.Security.Extensions.UnitTests;

public class LocalSigningKeyProviderTests
{
    private static JsonWebKey CreateTestJsonWebKey()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = ecdsa.ExportParameters(false);

        return new JsonWebKey
        {
            X = Base64UrlEncoder.Encode(p.Q.X!),
            Y = Base64UrlEncoder.Encode(p.Q.Y!),
            Kid = "local-kid",
            Crv = "P-256"
        };
    }

    [Fact]
    public async Task GetSigningKeysAsync_ReturnsSingleKeyFromLocalProvider()
    {
        var jwk = CreateTestJsonWebKey();
        var mockKeyProvider = new Mock<IJwtKeyProvider>();
        mockKeyProvider.Setup(x => x.GetJsonWebKey()).Returns(jwk);

        var provider = new LocalSigningKeyProvider(mockKeyProvider.Object);

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().ContainSingle();
        var key = keys.First().Should().BeOfType<ECDsaSecurityKey>().Subject;
        key.KeyId.Should().Be("local-kid");
    }

    [Fact]
    public async Task GetSigningKeysAsync_CallsGetJsonWebKeyEachTime()
    {
        var jwk = CreateTestJsonWebKey();
        var mockKeyProvider = new Mock<IJwtKeyProvider>();
        mockKeyProvider.Setup(x => x.GetJsonWebKey()).Returns(jwk);

        var provider = new LocalSigningKeyProvider(mockKeyProvider.Object);

        await provider.GetSigningKeysAsync();
        await provider.GetSigningKeysAsync();

        mockKeyProvider.Verify(x => x.GetJsonWebKey(), Times.Exactly(2));
    }
}

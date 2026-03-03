namespace Fudie.Security.Extensions.UnitTests;

public class JwksSigningKeyProviderTests
{
    private static JwkEntry CreateTestJwkEntry(string curve = "P-256")
    {
        var ecdsa = ECDsa.Create(curve switch
        {
            "P-256" => ECCurve.NamedCurves.nistP256,
            "P-384" => ECCurve.NamedCurves.nistP384,
            "P-521" => ECCurve.NamedCurves.nistP521,
            _ => throw new ArgumentException(curve)
        });

        var parameters = ecdsa.ExportParameters(false);
        return new JwkEntry(
            "EC", curve,
            Base64UrlEncoder.Encode(parameters.Q.X!),
            Base64UrlEncoder.Encode(parameters.Q.Y!),
            "test-kid", "sig", "ES256");
    }

    private static JwksSigningKeyProvider CreateProvider(
        Mock<IJwksApi> jwksApiMock,
        int cacheMinutes = 60)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new FudieSecurityOptions { CacheRefreshMinutes = cacheMinutes });
        return new JwksSigningKeyProvider(jwksApiMock.Object, cache, options);
    }

    [Fact]
    public async Task GetSigningKeysAsync_ReturnsKeysFromApi()
    {
        var jwkEntry = CreateTestJwkEntry();
        var jwksApiMock = new Mock<IJwksApi>();
        jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([jwkEntry]));

        var provider = CreateProvider(jwksApiMock);

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().ContainSingle();
        var key = keys.First().Should().BeOfType<ECDsaSecurityKey>().Subject;
        key.KeyId.Should().Be("test-kid");
    }

    [Fact]
    public async Task GetSigningKeysAsync_CachesResults()
    {
        var jwkEntry = CreateTestJwkEntry();
        var jwksApiMock = new Mock<IJwksApi>();
        jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([jwkEntry]));

        var provider = CreateProvider(jwksApiMock);

        await provider.GetSigningKeysAsync();
        await provider.GetSigningKeysAsync();

        jwksApiMock.Verify(x => x.GetJwksAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSigningKeysAsync_WithEmptyKeys_ReturnsEmpty()
    {
        var jwksApiMock = new Mock<IJwksApi>();
        jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([]));

        var provider = CreateProvider(jwksApiMock);

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSigningKeysAsync_WithMultipleKeys_ReturnsAll()
    {
        var entry1 = CreateTestJwkEntry("P-256");
        var entry2 = CreateTestJwkEntry("P-384");
        var jwksApiMock = new Mock<IJwksApi>();
        jwksApiMock.Setup(x => x.GetJwksAsync())
            .ReturnsAsync(new JwksResponse([entry1, entry2]));

        var provider = CreateProvider(jwksApiMock);

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSigningKeysAsync_WhenCacheHoldsNull_ReturnsEmpty()
    {
        var jwksApiMock = new Mock<IJwksApi>();
        var cacheMock = new Mock<IMemoryCache>();

        // TryGetValue returns true with null value — forces the ?? [] branch
        object? nullValue = null;
        cacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out nullValue))
            .Returns(true);

        var options = Options.Create(new FudieSecurityOptions { CacheRefreshMinutes = 60 });
        var provider = new JwksSigningKeyProvider(jwksApiMock.Object, cacheMock.Object, options);

        var keys = await provider.GetSigningKeysAsync();

        keys.Should().BeEmpty();
    }

}

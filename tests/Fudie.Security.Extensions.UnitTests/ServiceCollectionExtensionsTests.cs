namespace Fudie.Security.Extensions.UnitTests;

public class ServiceCollectionExtensionsTests
{
    private static IServiceCollection CreateServicesWithConfig(
        Dictionary<string, string?> configValues)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        return services;
    }

    [Fact]
    public void AddFudieJwksProvider_WithValidConfig_RegistersServices()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>
        {
            ["Fudie:Security:JwksUrl"] = "http://localhost/jwks",
            ["Fudie:Security:CacheRefreshMinutes"] = "30"
        });

        services.AddFudieJwksProvider();

        var provider = services.BuildServiceProvider();
        provider.GetService<ISigningKeyProvider>().Should().NotBeNull();
        provider.GetService<IJwtValidator>().Should().NotBeNull();
        provider.GetService<IMemoryCache>().Should().NotBeNull();

        var opts = provider.GetRequiredService<IOptions<FudieSecurityOptions>>().Value;
        opts.JwksUrl.Should().Be("http://localhost/jwks");
        opts.CacheRefreshMinutes.Should().Be(30);
    }

    [Fact]
    public void AddFudieJwksProvider_WithMissingSection_ThrowsInvalidOperation()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>());

        var act = () => services.AddFudieJwksProvider();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Fudie:Security*missing*");
    }

    [Fact]
    public void AddFudieJwksProvider_WithEmptyJwksUrl_ThrowsInvalidOperation()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>
        {
            ["Fudie:Security:CacheRefreshMinutes"] = "60"
        });

        var act = () => services.AddFudieJwksProvider();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwksUrl*required*");
    }

    [Fact]
    public void AddFudieJwksProvider_WithWhitespaceJwksUrl_ThrowsInvalidOperation()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>
        {
            ["Fudie:Security:JwksUrl"] = "   "
        });

        var act = () => services.AddFudieJwksProvider();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwksUrl*required*");
    }

    [Fact]
    public void AddFudieJwksProvider_ReturnsSameServiceCollection()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>
        {
            ["Fudie:Security:JwksUrl"] = "http://localhost/jwks"
        });

        var result = services.AddFudieJwksProvider();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddFudieJwksProvider_WithNoConfigurationRegistered_Throws()
    {
        var services = new ServiceCollection();

        var act = () => services.AddFudieJwksProvider();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddFudieJwksProvider_ResolvesConfigViaBuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:Security:JwksUrl"] = "http://localhost/jwks"
            })
            .Build();

        var services = new ServiceCollection();
        // Register as factory so ImplementationInstance is null — forces BuildServiceProvider path
        services.AddSingleton<IConfiguration>(_ => configuration);

        services.AddFudieJwksProvider();

        var provider = services.BuildServiceProvider();
        provider.GetService<ISigningKeyProvider>().Should().NotBeNull();
    }

    [Fact]
    public void AddFudieJwksProvider_DoesNotOverrideExistingSigningKeyProvider()
    {
        var services = CreateServicesWithConfig(new Dictionary<string, string?>
        {
            ["Fudie:Security:JwksUrl"] = "http://localhost/jwks"
        });
        var existing = new Mock<ISigningKeyProvider>();
        services.AddSingleton(existing.Object);

        services.AddFudieJwksProvider();

        var provider = services.BuildServiceProvider();
        provider.GetService<ISigningKeyProvider>().Should().BeSameAs(existing.Object);
    }

    [Fact]
    public void AddFudieLocalJwtValidation_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<IJwtKeyProvider>().Object);

        services.AddFudieLocalJwtValidation();

        var provider = services.BuildServiceProvider();
        provider.GetService<ISigningKeyProvider>().Should().BeOfType<LocalSigningKeyProvider>();
        provider.GetService<IJwtValidator>().Should().NotBeNull();
    }

    [Fact]
    public void AddFudieLocalJwtValidation_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddFudieLocalJwtValidation();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddFudieLocalJwtValidation_DoesNotOverrideExistingSigningKeyProvider()
    {
        var services = new ServiceCollection();
        var existing = new Mock<ISigningKeyProvider>();
        services.AddSingleton(existing.Object);

        services.AddFudieLocalJwtValidation();

        var provider = services.BuildServiceProvider();
        provider.GetService<ISigningKeyProvider>().Should().BeSameAs(existing.Object);
    }
}


namespace Fudie.Security.Api.UnitTests;

public class CatalogEndpointExtensionsTests
{
    #region MapCatalog Tests - Endpoint Registration

    [Fact]
    public void MapCatalog_ShouldRegisterGetCatalogEndpoint()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapCatalog();

        // Assert
        var endpoints = builder.DataSources
            .SelectMany(ds => ds.Endpoints)
            .ToList();
        endpoints.Should().HaveCount(1);
    }

    [Fact]
    public void MapCatalog_ShouldMarkEndpointAsInternal()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapCatalog();

        // Assert
        var endpoint = builder.DataSources
            .SelectMany(ds => ds.Endpoints).First();
        endpoint.Metadata
            .GetMetadata<InternalRequirement>()
            .Should().NotBeNull();
    }

    [Fact]
    public async Task MapCatalog_ShouldReturnCatalogResponse()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(CreateConfiguration("svc-1", "My Service"));
        var sp = services.BuildServiceProvider();

        var builder = new DefaultEndpointRouteBuilder(new ApplicationBuilder(sp));
        builder.MapCatalog();

        var endpoint = builder.DataSources.SelectMany(ds => ds.Endpoints).First() as RouteEndpoint;
        endpoint.Should().NotBeNull();

        var context = new DefaultHttpContext { RequestServices = sp };
        context.Response.Body = new MemoryStream();

        await endpoint!.RequestDelegate!(context);

        context.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(context.Response.Body);
        json.RootElement.GetProperty("serviceId").GetString().Should().Be("svc-1");
        json.RootElement.GetProperty("serviceName").GetString().Should().Be("My Service");
        json.RootElement.GetProperty("entries").GetArrayLength().Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(CreateConfiguration("test-service", "Test Service"));
        var serviceProvider = services.BuildServiceProvider();

        return new DefaultEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));
    }

    private static IConfiguration CreateConfiguration(string serviceId, string serviceName)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:ServiceId"] = serviceId,
                ["Fudie:ServiceName"] = serviceName
            })
            .Build();
    }

    private class DefaultEndpointRouteBuilder : IEndpointRouteBuilder
    {
        private readonly IApplicationBuilder _applicationBuilder;

        public DefaultEndpointRouteBuilder(IApplicationBuilder applicationBuilder)
        {
            _applicationBuilder = applicationBuilder;
            DataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder()
            => _applicationBuilder.New();

        public ICollection<EndpointDataSource> DataSources { get; }
        public IServiceProvider ServiceProvider => _applicationBuilder.ApplicationServices;
    }

    #endregion
}

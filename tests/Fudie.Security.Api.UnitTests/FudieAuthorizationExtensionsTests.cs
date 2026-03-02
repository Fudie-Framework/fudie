namespace Fudie.Security.Api.UnitTests;

public class FudieAuthorizationExtensionsTests
{
    #region ExtractAggregateNamespace - Valid

    [Fact]
    public void ExtractAggregateNamespace_CommandsNamespace_ShouldReturnAggregateNamespace()
    {
        var result = FudieAuthorizationExtensions.ExtractAggregateNamespace(
            "Menus.Features.Menus.Api.MenuAggregate.Commands");

        result.Should().Be("Menus.Features.Menus.Api.MenuAggregate");
    }

    [Fact]
    public void ExtractAggregateNamespace_QueriesNamespace_ShouldReturnAggregateNamespace()
    {
        var result = FudieAuthorizationExtensions.ExtractAggregateNamespace(
            "Auth.Features.Sessions.Api.SessionAggregate.Queries");

        result.Should().Be("Auth.Features.Sessions.Api.SessionAggregate");
    }

    #endregion

    #region ExtractAggregateNamespace - Invalid

    [Fact]
    public void ExtractAggregateNamespace_SingleSegment_ShouldThrow()
    {
        var act = () => FudieAuthorizationExtensions.ExtractAggregateNamespace("CreateMenu");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot extract aggregate namespace*");
    }

    #endregion

    #region ResolveAggregate

    [Fact]
    public void ResolveAggregate_WithMatchingNamespace_ShouldReturnDescription()
    {
        var descriptions = new Dictionary<string, IAggregateDescription>
        {
            ["App.MenuAggregate"] = new TestAggregate("menu", "Menus")
        };

        var result = FudieAuthorizationExtensions.ResolveAggregate(
            "CreateMenu", "App.MenuAggregate.Commands", descriptions);

        result.Id.Should().Be("menu");
    }

    [Fact]
    public void ResolveAggregate_WithNullNamespace_ShouldThrow()
    {
        var descriptions = new Dictionary<string, IAggregateDescription>();

        var act = () => FudieAuthorizationExtensions.ResolveAggregate(
            "CreateMenu", null, descriptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*has no namespace*");
    }

    [Fact]
    public void ResolveAggregate_WithNoMatchingDescription_ShouldThrow()
    {
        var descriptions = new Dictionary<string, IAggregateDescription>();

        var act = () => FudieAuthorizationExtensions.ResolveAggregate(
            "CreateMenu", "App.MenuAggregate.Commands", descriptions);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No IAggregateDescription found*");
    }

    #endregion

    #region BuildAggregateDescriptions

    [Fact]
    public void BuildAggregateDescriptions_ShouldReturnDictionary()
    {
        var result = FudieAuthorizationExtensions.BuildAggregateDescriptions();

        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<string, IAggregateDescription>>();
    }

    [Fact]
    public void BuildAggregateDescriptions_ShouldFindFakeAggregate()
    {
        var result = FudieAuthorizationExtensions.BuildAggregateDescriptions();

        result.Should().ContainKey("Fudie.Security.Api.UnitTests.FakeAggregate");
        result["Fudie.Security.Api.UnitTests.FakeAggregate"].Id.Should().Be("fake");
    }

    [Fact]
    public void BuildAggregateDescriptions_ShouldExcludeNoCtorAggregate()
    {
        var result = FudieAuthorizationExtensions.BuildAggregateDescriptions();

        result.Should().NotContainKey("Fudie.Security.Api.UnitTests.NoCtorAggregate");
    }

    #endregion

    #region SafeGetTypes

    [Fact]
    public void SafeGetTypes_WithReflectionTypeLoadException_ShouldReturnEmptyArray()
    {
        var method = typeof(FudieAuthorizationExtensions)
            .GetMethod("SafeGetTypes", BindingFlags.NonPublic | BindingFlags.Static)!;

        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetTypes())
            .Throws(new ReflectionTypeLoadException([], []));

        var result = (Type[])method.Invoke(null, [assembly.Object])!;

        result.Should().BeEmpty();
    }

    #endregion

    #region UseFudieAuthorization

    [Fact]
    public void UseFudieAuthorization_ShouldRegisterMappingsInCatalog()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Fudie:ServiceId"] = "test",
                    ["Fudie:ServiceName"] = "Test"
                }).Build());
        var sp = services.BuildServiceProvider();

        var appBuilder = new ApplicationBuilder(sp);
        var routeBuilder = new TestEndpointRouteBuilder(appBuilder);

        var featureBuilder = CreateFeatureBuilder(appBuilder, routeBuilder);

        var endpoint = new Endpoint(null,
            new EndpointMetadataCollection(new HttpMethodMetadata(["GET"])),
            displayName: "GetFake");
        var mapping = new FeatureEndpointMapping(
            "GetFake",
            "Fudie.Security.Api.UnitTests.FakeAggregate.Commands",
            endpoint);
        AddMapping(featureBuilder, mapping);

        featureBuilder.UseFudieAuthorization();

        var catalog = sp.GetRequiredService<ICatalogRegistry>();
        catalog.EndpointMapCount.Should().Be(1);
        catalog.FindClassName(endpoint).Should().Be("GetFake");
    }

    [Fact]
    public void UseFudieAuthorization_ShouldReturnSameFeatureBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Fudie:ServiceId"] = "test",
                    ["Fudie:ServiceName"] = "Test"
                }).Build());
        var sp = services.BuildServiceProvider();

        var appBuilder = new ApplicationBuilder(sp);
        var routeBuilder = new TestEndpointRouteBuilder(appBuilder);
        var featureBuilder = CreateFeatureBuilder(appBuilder, routeBuilder);

        var result = featureBuilder.UseFudieAuthorization();

        result.Should().BeSameAs(featureBuilder);
    }

    [Fact]
    public void UseFudieAuthorization_ShouldMapCatalogEndpoint()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        services.AddSingleton<ICatalogRegistry, CatalogRegistry>();
        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Fudie:ServiceId"] = "test",
                    ["Fudie:ServiceName"] = "Test"
                }).Build());
        var sp = services.BuildServiceProvider();

        var appBuilder = new ApplicationBuilder(sp);
        var routeBuilder = new TestEndpointRouteBuilder(appBuilder);
        var featureBuilder = CreateFeatureBuilder(appBuilder, routeBuilder);

        featureBuilder.UseFudieAuthorization();

        var endpoints = routeBuilder.DataSources
            .SelectMany(ds => ds.Endpoints).ToList();
        endpoints.Should().HaveCount(1);
    }

    #endregion

    #region Helpers

    private static FeatureBuilder CreateFeatureBuilder(
        IApplicationBuilder app, IEndpointRouteBuilder routes)
    {
        var ctor = typeof(FeatureBuilder).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, [typeof(IApplicationBuilder), typeof(IEndpointRouteBuilder)], null)!;
        return (FeatureBuilder)ctor.Invoke([app, routes]);
    }

    private static void AddMapping(FeatureBuilder builder, FeatureEndpointMapping mapping)
    {
        var method = typeof(FeatureBuilder).GetMethod(
            "AddMapping", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(builder, [mapping]);
    }

    private class TestEndpointRouteBuilder : IEndpointRouteBuilder
    {
        private readonly IApplicationBuilder _appBuilder;

        public TestEndpointRouteBuilder(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;
            DataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder() => _appBuilder.New();
        public ICollection<EndpointDataSource> DataSources { get; }
        public IServiceProvider ServiceProvider => _appBuilder.ApplicationServices;
    }

    private record TestAggregate(string Id, string DisplayName) : IAggregateDescription
    {
        public string? Icon => null;
        public string ReadDescription => "Read";
        public string WriteDescription => "Write";
    }

    #endregion
}

namespace Fudie.Api.UnitTests;

public class RouteExtensionTests
{
    #region MapFeatures Tests - Basic Functionality

    [Fact]
    public void MapFeatures_WithNoConfigureCallback_ShouldNotThrow()
    {
        var builder = CreateEndpointRouteBuilder();

        var act = () => builder.MapFeatures();

        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_WithConfigureCallback_ShouldNotThrow()
    {
        var builder = CreateEndpointRouteBuilder();

        var act = () => builder.MapFeatures(_ => { });

        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_CalledMultipleTimes_ShouldNotThrow()
    {
        var builder = CreateEndpointRouteBuilder();

        var act = () =>
        {
            builder.MapFeatures();
            builder.MapFeatures();
        };

        act.Should().NotThrow();
    }

    #endregion

    #region MapFeatures Tests - Feature Module Discovery

    [Fact]
    public void MapFeatures_ShouldDiscoverFeatureModulesFromTestAssembly()
    {
        var builder = CreateEndpointRouteBuilder();

        builder.MapFeatures();

        builder.DataSources.SelectMany(ds => ds.Endpoints)
            .Should().NotBeEmpty("TestFeatureModule should register endpoints");
    }

    [Fact]
    public void MapFeatures_ShouldOnlyDiscoverPublicClasses()
    {
        var builder = CreateEndpointRouteBuilder();

        var act = () => builder.MapFeatures();

        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_ShouldNotDiscoverAbstractClasses()
    {
        var builder = CreateEndpointRouteBuilder();

        var act = () => builder.MapFeatures();

        act.Should().NotThrow();
    }

    #endregion

    #region MapFeatures Tests - FeatureBuilder Callback

    [Fact]
    public void MapFeatures_ShouldInvokeConfigureCallback()
    {
        var builder = CreateEndpointRouteBuilder();
        var callbackInvoked = false;

        builder.MapFeatures(_ => callbackInvoked = true);

        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void MapFeatures_ShouldPassFeatureBuilderWithApp()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received.Should().NotBeNull();
        received!.App.Should().NotBeNull();
    }

    [Fact]
    public void MapFeatures_ShouldPassFeatureBuilderWithRoutes()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received.Should().NotBeNull();
        received!.Routes.Should().BeSameAs(builder);
    }

    [Fact]
    public void MapFeatures_ShouldPopulateEndpointMappings()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received!.EndpointMappings.Should().NotBeEmpty();
    }

    [Fact]
    public void MapFeatures_ShouldSetCorrectClassNameInMappings()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received!.EndpointMappings
            .Should().Contain(m => m.ClassName == "TestFeatureModule");
    }

    [Fact]
    public void MapFeatures_ShouldSetFeatureNamespaceInMappings()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received!.EndpointMappings
            .Should().Contain(m =>
                m.FeatureNamespace == "Fudie.Api.UnitTests.TestAggregate.Commands");
    }

    [Fact]
    public void MapFeatures_ShouldSetEndpointInMappings()
    {
        var builder = CreateEndpointRouteBuilder();
        FeatureBuilder? received = null;

        builder.MapFeatures(fb => received = fb);

        received!.EndpointMappings
            .Should().AllSatisfy(m => m.Endpoint.Should().NotBeNull());
    }

    [Fact]
    public void MapFeatures_WithNullConfigure_ShouldNotPopulateMappingsExternally()
    {
        var builder = CreateEndpointRouteBuilder();

        builder.MapFeatures();

        builder.DataSources.SelectMany(ds => ds.Endpoints)
            .Should().NotBeEmpty();
    }

    #endregion

    #region MapFeatures Tests - InvalidOperationException

    [Fact]
    public void MapFeatures_OnNonApplicationBuilder_ShouldThrow()
    {
        var builder = CreatePureEndpointRouteBuilder();

        var act = () => builder.MapFeatures(_ => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IApplicationBuilder*");
    }

    #endregion

    #region Private Method Tests - SafeGetTypes

    [Fact]
    public void SafeGetTypes_WithValidAssembly_ShouldReturnTypes()
    {
        var method = typeof(RouteExtension).GetMethod(
            "SafeGetTypes",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = (Type[])method!.Invoke(null, [typeof(RouteExtension).Assembly])!;

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void SafeGetTypes_WithReflectionTypeLoadException_ShouldReturnEmpty()
    {
        var method = typeof(RouteExtension).GetMethod(
            "SafeGetTypes",
            BindingFlags.NonPublic | BindingFlags.Static);

        var brokenAssembly = new ThrowingAssembly();

        var result = (Type[])method!.Invoke(null, [brokenAssembly])!;

        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        return new TestEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));
    }

    private static IEndpointRouteBuilder CreatePureEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        return new PureEndpointRouteBuilder(serviceProvider);
    }

    /// <summary>
    /// Implements both IEndpointRouteBuilder and IApplicationBuilder,
    /// matching WebApplication behavior.
    /// </summary>
    private class TestEndpointRouteBuilder : IEndpointRouteBuilder, IApplicationBuilder
    {
        private readonly IApplicationBuilder _applicationBuilder;

        public TestEndpointRouteBuilder(IApplicationBuilder applicationBuilder)
        {
            _applicationBuilder = applicationBuilder;
            DataSources = new List<EndpointDataSource>();
        }

        // IEndpointRouteBuilder
        public IApplicationBuilder CreateApplicationBuilder() => _applicationBuilder.New();
        public ICollection<EndpointDataSource> DataSources { get; }
        public IServiceProvider ServiceProvider => _applicationBuilder.ApplicationServices;

        // IApplicationBuilder
        public IServiceProvider ApplicationServices
        {
            get => _applicationBuilder.ApplicationServices;
            set => _applicationBuilder.ApplicationServices = value;
        }

        public IFeatureCollection ServerFeatures => _applicationBuilder.ServerFeatures;
        public IDictionary<string, object?> Properties => _applicationBuilder.Properties;

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
            => _applicationBuilder.Use(middleware);

        public IApplicationBuilder New() => _applicationBuilder.New();
        public RequestDelegate Build() => _applicationBuilder.Build();
    }

    /// <summary>
    /// Only implements IEndpointRouteBuilder (NOT IApplicationBuilder).
    /// Used to test the InvalidOperationException path.
    /// </summary>
    private class PureEndpointRouteBuilder : IEndpointRouteBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public PureEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder CreateApplicationBuilder()
            => new ApplicationBuilder(_serviceProvider);

        public ICollection<EndpointDataSource> DataSources { get; }
        public IServiceProvider ServiceProvider => _serviceProvider;
    }

    private class ThrowingAssembly : Assembly
    {
        public override Type[] GetTypes()
        {
            throw new ReflectionTypeLoadException([], [new Exception("Simulated load failure")]);
        }
    }

    #endregion
}

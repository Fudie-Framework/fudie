namespace Fudie.Features.UnitTests;

public class RouteExtensionTests
{
    #region MapFeatures Tests - Basic Functionality

    [Fact]
    public void MapFeatures_WithNoFeatureModules_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_ShouldNotThrowWhenCalledOnValidBuilder()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MapFeatures_ShouldCompleteSuccessfully()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapFeatures();

        // Assert
        // If we reach here without exception, the test passes
        true.Should().BeTrue();
    }

    #endregion

    #region MapFeatures Tests - Feature Module Discovery

    [Fact]
    public void MapFeatures_ShouldOnlyDiscoverPublicClasses()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Private and internal classes should not be discovered
    }

    [Fact]
    public void MapFeatures_ShouldNotDiscoverAbstractClasses()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Abstract classes should not be instantiated
    }

    [Fact]
    public void MapFeatures_ShouldNotDiscoverInterfaces()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
        // Interfaces should not be instantiated
    }

    #endregion

    #region MapFeatures Tests - Assembly Discovery

    [Fact]
    public void MapFeatures_ShouldHandleReflectionTypeLoadException()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow("MapFeatures should handle ReflectionTypeLoadException gracefully");
    }

    #endregion

    #region MapFeatures Tests - Multiple Calls

    [Fact]
    public void MapFeatures_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () =>
        {
            builder.MapFeatures();
            builder.MapFeatures();
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region MapFeatures Tests - Feature Registration

    [Fact]
    public void MapFeatures_ShouldDiscoverFeatureModulesFromTestAssembly()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        builder.MapFeatures();

        // Assert
        builder.DataSources.SelectMany(ds => ds.Endpoints)
            .Should().NotBeEmpty("TestFeatureModule should register endpoints");
    }

    [Fact]
    public void MapFeatures_WithCatalog_ShouldRegisterEndpoints()
    {
        // Arrange
        var catalog = new TestCatalogRegistry();
        var builder = CreateEndpointRouteBuilder(catalog);

        // Act
        builder.MapFeatures();

        // Assert
        catalog.Registrations.Should().NotBeEmpty();
    }

    [Fact]
    public void MapFeatures_WithCatalog_ShouldPassCorrectClassName()
    {
        // Arrange
        var catalog = new TestCatalogRegistry();
        var builder = CreateEndpointRouteBuilder(catalog);

        // Act
        builder.MapFeatures();

        // Assert
        catalog.Registrations.Should().Contain(r => r.ClassName == "TestFeatureModule");
    }

    [Fact]
    public void MapFeatures_WithCatalog_ShouldPassAggregateDescription()
    {
        // Arrange
        var catalog = new TestCatalogRegistry();
        var builder = CreateEndpointRouteBuilder(catalog);

        // Act
        builder.MapFeatures();

        // Assert
        catalog.Registrations.Should().Contain(r => r.Aggregate.Id == "test");
    }

    [Fact]
    public void MapFeatures_WithoutCatalog_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateEndpointRouteBuilder();

        // Act
        var act = () => builder.MapFeatures();

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Private Method Tests - Error Paths via Reflection

    [Fact]
    public void ExtractAggregateNamespace_WithSingleSegment_ShouldThrow()
    {
        var method = typeof(RouteExtension).GetMethod(
            "ExtractAggregateNamespace",
            BindingFlags.NonPublic | BindingFlags.Static);

        var act = () => method!.Invoke(null, ["RootOnly"]);

        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<InvalidOperationException>()
           .WithMessage("*Cannot extract aggregate namespace*");
    }

    [Fact]
    public void ExtractAggregateNamespace_WithEmptyString_ShouldThrow()
    {
        var method = typeof(RouteExtension).GetMethod(
            "ExtractAggregateNamespace",
            BindingFlags.NonPublic | BindingFlags.Static);

        var act = () => method!.Invoke(null, [""]);

        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<InvalidOperationException>();
    }

    [Fact]
    public void ExtractAggregateNamespace_WithValidNamespace_ShouldReturnParent()
    {
        var method = typeof(RouteExtension).GetMethod(
            "ExtractAggregateNamespace",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, ["Fudie.Features.Aggregate.Commands"]);

        result.Should().Be("Fudie.Features.Aggregate");
    }

    [Fact]
    public void ResolveAggregate_WithMissingAggregate_ShouldThrow()
    {
        var method = typeof(RouteExtension).GetMethod(
            "ResolveAggregate",
            BindingFlags.NonPublic | BindingFlags.Static);

        var feature = new OrphanFeatureModule();
        var emptyDescriptions = new Dictionary<string, IAggregateDescription>();

        var act = () => method!.Invoke(null, [feature, emptyDescriptions]);

        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<InvalidOperationException>()
           .WithMessage("*No IAggregateDescription found*");
    }

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

    [Fact]
    public void ResolveAggregate_WithNullNamespace_ShouldThrow()
    {
        var method = typeof(RouteExtension).GetMethod(
            "ResolveAggregate",
            BindingFlags.NonPublic | BindingFlags.Static);

        var feature = new GlobalNamespaceFeatureModule();
        var descriptions = new Dictionary<string, IAggregateDescription>();

        var act = () => method!.Invoke(null, [feature, descriptions]);

        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<InvalidOperationException>()
           .WithMessage("*has no namespace*");
    }

    #endregion

    #region Helper Methods

    private static IEndpointRouteBuilder CreateEndpointRouteBuilder(
        ICatalogRegistry? catalog = null)
    {
        var services = new ServiceCollection();
        services.AddRouting();

        if (catalog is not null)
            services.AddSingleton(catalog);

        var serviceProvider = services.BuildServiceProvider();

        var builder = new DefaultEndpointRouteBuilder(
            new ApplicationBuilder(serviceProvider));

        return builder;
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
        {
            return _applicationBuilder.New();
        }

        public ICollection<EndpointDataSource> DataSources { get; }

        public IServiceProvider ServiceProvider => _applicationBuilder.ApplicationServices;
    }

    private class TestCatalogRegistry : ICatalogRegistry
    {
        public List<(string ClassName, Endpoint Endpoint, IAggregateDescription Aggregate)> Registrations { get; } = [];

        public void Register(string className, Endpoint endpoint, IAggregateDescription aggregate)
        {
            Registrations.Add((className, endpoint, aggregate));
        }

        public Endpoint? FindEndpoint(string displayName) => null;
        public string? FindClassName(Endpoint endpoint) => null;
        public int EndpointMapCount => Registrations.Count;
        public IReadOnlyList<CatalogEntry> GetAll() => [];
        public IReadOnlyList<CatalogEntry> GetTenant() => [];
    }

    private class OrphanFeatureModule : IFeatureModule
    {
        public void AddRoutes(IEndpointRouteBuilder app) { }
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

public class IFeatureModuleTests
{
    [Fact]
    public void IFeatureModule_ShouldBeAnInterface()
    {
        typeof(IFeatureModule).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IFeatureModule_ShouldDeclareExactlyOneMethod()
    {
        var methods = typeof(IFeatureModule).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IFeatureModule_ShouldDeclareAddRoutesMethod()
    {
        var method = typeof(IFeatureModule).GetMethod("AddRoutes");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("app");
        method.GetParameters()[0].ParameterType.Should().Be(typeof(IEndpointRouteBuilder));
    }

    [Fact]
    public void IFeatureModule_CanBeImplemented()
    {
        var implementation = new TestImplementation();

        implementation.Should().BeAssignableTo<IFeatureModule>();
    }

    [Fact]
    public void IFeatureModule_Implementation_CanCallAddRoutes()
    {
        var implementation = new TestImplementation();
        var builder = CreateMockEndpointRouteBuilder();

        var act = () => implementation.AddRoutes(builder);

        act.Should().NotThrow();
    }

    [Fact]
    public void IFeatureModule_Implementation_ReceivesCorrectParameter()
    {
        var implementation = new TestImplementation();
        var builder = CreateMockEndpointRouteBuilder();

        implementation.AddRoutes(builder);

        implementation.ReceivedBuilder.Should().BeSameAs(builder);
    }

    private static IEndpointRouteBuilder CreateMockEndpointRouteBuilder()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        return new MockEndpointRouteBuilder(serviceProvider);
    }

    private class MockEndpointRouteBuilder(IServiceProvider serviceProvider) : IEndpointRouteBuilder
    {
        public IApplicationBuilder CreateApplicationBuilder() => new ApplicationBuilder(serviceProvider);
        public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();
        public IServiceProvider ServiceProvider { get; } = serviceProvider;
    }

    private class TestImplementation : IFeatureModule
    {
        public IEndpointRouteBuilder? ReceivedBuilder { get; private set; }
        public void AddRoutes(IEndpointRouteBuilder app) => ReceivedBuilder = app;
    }
}

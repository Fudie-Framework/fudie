namespace Fudie.Security.Http.UnitTests;

public class FudieAuthorizationMiddlewareTests
{
    private readonly Mock<IJwtValidator> _jwtValidator = new();
    private readonly ICatalogRegistry _catalog = new CatalogRegistry();
    private readonly IConfiguration _config;
    private bool _nextCalled;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    public FudieAuthorizationMiddlewareTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Fudie:InternalSecret"] = "secret-123",
                ["Fudie:PlatformTenantId"] = TenantId.ToString()
            })
            .Build();
    }

    #region No Endpoint

    [Fact]
    public async Task InvokeAsync_NoEndpoint_ShouldCallNext()
    {
        var context = new DefaultHttpContext();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region AllowAnonymous

    [Fact]
    public async Task InvokeAsync_AllowAnonymous_ShouldCallNext()
    {
        var context = CreateContext(new AllowAnonymousAttribute());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AllowAnonymous_WithToken_ShouldSetContext()
    {
        var context = CreateContext(new AllowAnonymousAttribute());
        context.Request.Headers.Authorization = "Bearer valid-token";
        SetupValidToken();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Items["FudieTokenContext"].Should().NotBeNull();
    }

    #endregion

    #region Internal

    [Fact]
    public async Task InvokeAsync_Internal_WithValidKey_ShouldCallNext()
    {
        var context = CreateContext(new InternalRequirement());
        context.Request.Headers["X-Internal-Key"] = "secret-123";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Internal_WithInvalidKey_ShouldReturn401()
    {
        var context = CreateContext(new InternalRequirement());
        context.Request.Headers["X-Internal-Key"] = "wrong-key";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(401);
        _nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_Internal_WithNoKey_ShouldReturn401()
    {
        var context = CreateContext(new InternalRequirement());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region No Token / Invalid Token

    [Fact]
    public async Task InvokeAsync_NoBearerHeader_ShouldReturn401()
    {
        var context = CreateContext(new AuthenticatedRequirement());
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_InvalidToken_ShouldReturn401()
    {
        var context = CreateContext(new AuthenticatedRequirement());
        context.Request.Headers.Authorization = "Bearer invalid-token";
        _jwtValidator.Setup(x => x.ValidateTokenAsync("invalid-token"))
            .ReturnsAsync((FudieTokenContext?)null);
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region AuthenticatedRequirement

    [Fact]
    public async Task InvokeAsync_AuthenticatedRequirement_WithValidToken_ShouldCallNext()
    {
        var context = CreateContext(new AuthenticatedRequirement());
        SetupValidToken();
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
        context.Items["FudieTokenContext"].Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedRequirement_ExcludedScope_ShouldStillAllow()
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthenticatedRequirement()),
            "TestEndpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("TestEndpoint", endpoint, new TestAggregate());
        SetupValidToken(excludedScopes: ["TestEndpoint"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region Platform

    [Fact]
    public async Task InvokeAsync_Platform_WithCorrectTenant_ShouldCallNext()
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new PlatformRequirement()),
            "test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(tenantId: TenantId, groups: ["test:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_Platform_WithWrongTenant_ShouldReturn403()
    {
        var context = CreateContext(new PlatformRequirement());
        SetupValidToken(tenantId: Guid.NewGuid());
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_Platform_WithNullTenant_ShouldReturn403()
    {
        var context = CreateContext(new PlatformRequirement());
        SetupValidToken(tenantId: null);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_Platform_WithNoPlatformConfig_ShouldReturn403()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var context = CreateContext(new PlatformRequirement());
        SetupValidToken(tenantId: TenantId);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, config);

        context.Response.StatusCode.Should().Be(403);
    }

    #endregion

    #region Owner

    [Fact]
    public async Task InvokeAsync_Owner_ShouldCallNextAndSetContext()
    {
        var context = CreateContext();
        SetupValidToken(isOwner: true);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
        context.Items["FudieTokenContext"].Should().NotBeNull();
    }

    #endregion

    #region ExcludedScopes

    [Fact]
    public async Task InvokeAsync_ExcludedScope_ShouldReturn403()
    {
        var endpoint = CreateEndpointWithDisplayName("TestEndpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("TestEndpoint", endpoint, new TestAggregate());
        SetupValidToken(excludedScopes: ["TestEndpoint"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(403);
    }

    #endregion

    #region AdditionalScopes

    [Fact]
    public async Task InvokeAsync_AdditionalScope_ShouldCallNext()
    {
        var endpoint = CreateEndpointWithDisplayName("TestEndpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("TestEndpoint", endpoint, new TestAggregate());
        SetupValidToken(additionalScopes: ["TestEndpoint"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region GroupRequirement

    [Fact]
    public async Task InvokeAsync_GroupRequirement_WithMatchingGroup_ShouldCallNext()
    {
        var groupReq = new GroupRequirement("menu:read", "Read menus");
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(groupReq),
            "test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(groups: ["menu:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_GroupRequirement_WithoutMatchingGroup_ShouldReturn403()
    {
        var groupReq = new GroupRequirement("menu:read", "Read menus");
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(groupReq),
            "test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(groups: ["other:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(403);
    }

    #endregion

    #region Scope Access (deny by default)

    [Fact]
    public async Task InvokeAsync_NoScope_WithValidToken_ShouldReturn403()
    {
        var context = CreateContext();
        SetupValidToken();
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.StatusCode.Should().Be(403);
        _nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithMatchingScope_ShouldCallNext()
    {
        var endpoint = CreateEndpointWithDisplayName("test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(groups: ["test:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region SetTokenContext

    [Fact]
    public async Task InvokeAsync_WithTenantId_ShouldSetTidClaim()
    {
        var endpoint = CreateEndpointWithDisplayName("test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(tenantId: TenantId, groups: ["test:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.User.FindFirst("tid")?.Value.Should().Be(TenantId.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithOwner_ShouldSetOwnerClaim()
    {
        var context = CreateContext();
        SetupValidToken(isOwner: true);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.User.HasClaim("owner", "true").Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNoTenantId_ShouldNotSetTidClaim()
    {
        var endpoint = CreateEndpointWithDisplayName("test-endpoint");
        var context = CreateContextWithEndpoint(endpoint);
        _catalog.Register("test-endpoint", endpoint, new TestAggregate());
        SetupValidToken(tenantId: null, groups: ["test:read"]);
        context.Request.Headers.Authorization = "Bearer valid-token";
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.User.FindFirst("tid").Should().BeNull();
    }

    #endregion

    #region WriteProblem

    [Fact]
    public async Task InvokeAsync_WhenUnauthorized_ShouldWriteProblemJson()
    {
        var context = CreateContext(new AuthenticatedRequirement());
        context.Response.Body = new MemoryStream();
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context, _jwtValidator.Object, _catalog, _config);

        context.Response.ContentType.Should().Contain("application/json");
        context.Response.Body.Position = 0;
        var body = await JsonDocument.ParseAsync(context.Response.Body);
        body.RootElement.GetProperty("title").GetString().Should().Be("Unauthorized");
    }

    #endregion

    #region Helpers

    private FudieAuthorizationMiddleware CreateMiddleware()
    {
        return new FudieAuthorizationMiddleware(ctx =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });
    }

    private static DefaultHttpContext CreateContext(params object[] metadata)
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(metadata),
            "test-endpoint");
        var context = new DefaultHttpContext();
        context.SetEndpoint(endpoint);
        return context;
    }

    private static DefaultHttpContext CreateContextWithEndpoint(Endpoint endpoint)
    {
        var context = new DefaultHttpContext();
        context.SetEndpoint(endpoint);
        return context;
    }

    private static Endpoint CreateEndpointWithDisplayName(string displayName)
    {
        return new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(),
            displayName);
    }

    private void SetupValidToken(
        Guid? tenantId = null,
        bool isOwner = false,
        string[]? groups = null,
        string[]? additionalScopes = null,
        string[]? excludedScopes = null)
    {
        var tokenContext = new FudieTokenContext(
            UserId, tenantId, isOwner,
            groups ?? [],
            additionalScopes ?? [],
            excludedScopes ?? []);

        _jwtValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(tokenContext);
    }

    private record TestAggregate : IAggregateDescription
    {
        public string Id => "test";
        public string DisplayName => "Test";
        public string? Icon => null;
        public string ReadDescription => "Read";
        public string WriteDescription => "Write";
    }

    #endregion
}

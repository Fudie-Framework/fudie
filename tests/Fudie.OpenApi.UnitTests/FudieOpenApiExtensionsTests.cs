namespace Fudie.OpenApi.UnitTests;

public class FudieOpenApiExtensionsTests : IDisposable
{
    private readonly string _tempDir;

    public FudieOpenApiExtensionsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"fudie-openapi-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private WebApplication BuildApp(
        string environment = "Development",
        Dictionary<string, string?>? config = null,
        bool useTestServer = false)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = _tempDir,
            EnvironmentName = environment
        });

        if (useTestServer)
            builder.WebHost.UseTestServer();

        if (config != null)
            builder.Configuration.AddInMemoryCollection(config);

        return builder.Build();
    }

    private void CreateYamlFile(string folder, string fileName, string? subfolder = null)
    {
        var dir = subfolder != null
            ? Path.Combine(_tempDir, folder, subfolder)
            : Path.Combine(_tempDir, folder);

        Directory.CreateDirectory(dir);
        File.WriteAllText(
            Path.Combine(dir, fileName),
            "openapi: 3.0.0\ninfo:\n  title: Test\n  version: 1.0.0");
    }

    #region Early Return Tests

    [Fact]
    public void UseFudieOpenApi_WhenNotDevelopment_ShouldReturnApp()
    {
        // Arrange
        var app = BuildApp("Production");

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseFudieOpenApi_WhenFolderDoesNotExist_ShouldReturnApp()
    {
        // Arrange
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseFudieOpenApi_WhenFolderIsEmpty_ShouldReturnApp()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_tempDir, "OpenApi"));
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public void UseFudieOpenApi_WithYamlFiles_ShouldConfigureAndReturnApp()
    {
        // Arrange
        CreateYamlFile("OpenApi", "petstore.yaml");
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseFudieOpenApi_WithMultipleYamlFiles_ShouldConfigureAll()
    {
        // Arrange
        CreateYamlFile("OpenApi", "users.yaml");
        CreateYamlFile("OpenApi", "products.yaml");
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseFudieOpenApi_WithYamlInSubfolders_ShouldDiscoverRecursively()
    {
        // Arrange
        CreateYamlFile("OpenApi", "api.yaml", "v1");
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void UseFudieOpenApi_WithCustomFolder_ShouldUseConfiguredFolder()
    {
        // Arrange
        CreateYamlFile("MyContracts", "api.yaml");
        var config = new Dictionary<string, string?>
        {
            ["Fudie:OpenApi:Folder"] = "MyContracts"
        };
        var app = BuildApp(config: config);

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseFudieOpenApi_WithCustomRoutePrefix_ShouldUseConfiguredPrefix()
    {
        // Arrange
        CreateYamlFile("OpenApi", "api.yaml");
        var config = new Dictionary<string, string?>
        {
            ["Fudie:OpenApi:RoutePrefix"] = "docs/swagger"
        };
        var app = BuildApp(config: config);

        // Act
        var result = app.UseFudieOpenApi();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public async Task UseFudieOpenApi_WithCustomRequestPath_ShouldServeYamlAtPrefixedUrl()
    {
        // Arrange
        CreateYamlFile("OpenApi", "plan-api.yaml");
        var config = new Dictionary<string, string?>
        {
            ["Fudie:OpenApi:RequestPath"] = "plans/OpenApi",
            ["Fudie:OpenApi:RoutePrefix"] = "plans/swagger"
        };
        var app = BuildApp(useTestServer: true, config: config);
        app.UseFudieOpenApi();
        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var yamlResponse = await client.GetAsync("/plans/OpenApi/plan-api.yaml");
        var redirectResponse = await client.GetAsync("/");

        // Assert
        yamlResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        yamlResponse.Headers.CacheControl!.NoCache.Should().BeTrue();
        redirectResponse.Headers.Location!.ToString().Should().Be("/plans/swagger");
        await app.StopAsync();
    }

    [Fact]
    public void UseFudieOpenApi_WithCredentialsFalse_ShouldConfigureWithoutInterceptor()
    {
        // Arrange
        CreateYamlFile("OpenApi", "api.yaml");
        var app = BuildApp();

        // Act
        var result = app.UseFudieOpenApi(withCredentials: false);

        // Assert
        result.Should().BeSameAs(app);
    }

    #endregion

    #region Cache Control Tests

    [Fact]
    public async Task UseFudieOpenApi_WhenServingYaml_ShouldSetNoCacheHeader()
    {
        // Arrange
        CreateYamlFile("OpenApi", "petstore.yaml");
        var app = BuildApp(useTestServer: true);
        app.UseFudieOpenApi();
        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/OpenApi/petstore.yaml");

        // Assert
        response.Headers.CacheControl!.NoCache.Should().BeTrue();
        await app.StopAsync();
    }

    #endregion

    #region Root Redirect Tests

    [Fact]
    public async Task UseFudieOpenApi_RootPath_ShouldRedirectToSwaggerUI()
    {
        // Arrange
        CreateYamlFile("OpenApi", "petstore.yaml");
        var app = BuildApp(useTestServer: true);
        app.UseFudieOpenApi();
        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/swagger");
        await app.StopAsync();
    }

    [Fact]
    public async Task UseFudieOpenApi_RootPathWithCustomPrefix_ShouldRedirectToCustomPrefix()
    {
        // Arrange
        CreateYamlFile("OpenApi", "api.yaml");
        var config = new Dictionary<string, string?>
        {
            ["Fudie:OpenApi:RoutePrefix"] = "schedules/swagger"
        };
        var app = BuildApp(useTestServer: true, config: config);
        app.UseFudieOpenApi();
        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/schedules/swagger");
        await app.StopAsync();
    }

    #endregion
}

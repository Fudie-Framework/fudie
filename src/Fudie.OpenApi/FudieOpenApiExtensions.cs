namespace Fudie.OpenApi;

/// <summary>
/// Extension method for <see cref="WebApplication"/> that autodiscovers
/// OpenAPI YAML contracts and configures SwaggerUI in development.
/// </summary>
public static class FudieOpenApiExtensions
{
    /// <summary>
    /// Reads the configured folder (default <c>OpenApi/</c>), serves <c>.yaml</c> files
    /// as static files with the correct content type, and configures SwaggerUI
    /// with all discovered contracts. Only active in the development environment.
    /// Static files are served with <c>Cache-Control: no-cache</c> so contract
    /// changes are reflected immediately without clearing the browser cache.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseFudieOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        var (folder, routePrefix) = ReadConfiguration(app);
        var openApiPath = Path.Combine(app.Environment.ContentRootPath, folder);
        var yamlFiles = DiscoverYamlContracts(openApiPath);

        if (yamlFiles.Length == 0)
            return app;

        RegisterMiddleware(app, openApiPath, folder, routePrefix, yamlFiles);

        return app;
    }

    /// <summary>
    /// Returns all <c>.yaml</c> files found recursively under <paramref name="openApiPath"/>,
    /// or an empty array if the folder does not exist.
    /// </summary>
    private static string[] DiscoverYamlContracts(string openApiPath)
    {
        if (!Directory.Exists(openApiPath))
            return [];

        return Directory.GetFiles(openApiPath, "*.yaml", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Reads the OpenApi folder and route prefix from configuration,
    /// falling back to <c>OpenApi</c> and <c>swagger</c> respectively.
    /// </summary>
    private static (string folder, string routePrefix) ReadConfiguration(WebApplication app)
    {
        var folder = app.Configuration.GetValue<string>("Fudie:OpenApi:Folder") ?? "OpenApi";
        var routePrefix = app.Configuration.GetValue<string>("Fudie:OpenApi:RoutePrefix") ?? "swagger";
        return (folder, routePrefix);
    }

    /// <summary>
    /// Registers static file serving for YAML contracts, configures SwaggerUI
    /// with discovered endpoints, and adds a root redirect to the Swagger UI.
    /// </summary>
    private static void RegisterMiddleware(
        WebApplication app,
        string openApiPath,
        string folder,
        string routePrefix,
        string[] yamlFiles)
    {
        var provider = new PhysicalFileProvider(openApiPath);
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings[".yaml"] = "application/x-yaml";

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = provider,
            RequestPath = $"/{folder}",
            ContentTypeProvider = contentTypeProvider,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.CacheControl = "no-cache";
            }
        });

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = routePrefix;

            foreach (var yamlFile in yamlFiles)
            {
                var relativePath = Path.GetRelativePath(openApiPath, yamlFile).Replace('\\', '/');
                var name = Path.GetFileNameWithoutExtension(yamlFile);
                options.SwaggerEndpoint($"/{folder}/{relativePath}", name);
            }
        });

        app.MapGet("/", () => Results.Redirect($"/{routePrefix}")).AllowAnonymous();
    }
}

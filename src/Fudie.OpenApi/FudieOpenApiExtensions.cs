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
    /// <param name="withCredentials">
    /// When <c>true</c> (default), configures SwaggerUI to send cookies with every request
    /// so cookie-based authentication works out of the box during development.
    /// </param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseFudieOpenApi(this WebApplication app, bool withCredentials = true)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        var (folder, requestPath, routePrefix) = ReadConfiguration(app);
        var openApiPath = Path.Combine(app.Environment.ContentRootPath, folder);
        var yamlFiles = DiscoverYamlContracts(openApiPath);

        if (yamlFiles.Length == 0)
            return app;

        RegisterMiddleware(app, openApiPath, requestPath, routePrefix, yamlFiles, withCredentials);

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
    /// Reads the OpenApi folder, request path, and route prefix from configuration,
    /// falling back to <c>OpenApi</c>, the folder value, and <c>swagger</c> respectively.
    /// </summary>
    private static (string folder, string requestPath, string routePrefix) ReadConfiguration(WebApplication app)
    {
        var folder = app.Configuration.GetValue<string>("Fudie:OpenApi:Folder") ?? "OpenApi";
        var requestPath = app.Configuration.GetValue<string>("Fudie:OpenApi:RequestPath") ?? folder;
        var routePrefix = app.Configuration.GetValue<string>("Fudie:OpenApi:RoutePrefix") ?? "swagger";
        return (folder, requestPath, routePrefix);
    }

    /// <summary>
    /// Registers static file serving for YAML contracts, configures SwaggerUI
    /// with discovered endpoints, and adds a root redirect to the Swagger UI.
    /// </summary>
    private static void RegisterMiddleware(
        WebApplication app,
        string openApiPath,
        string requestPath,
        string routePrefix,
        string[] yamlFiles,
        bool withCredentials)
    {
        var fileProvider = new PhysicalFileProvider(openApiPath);
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".yaml"] = "application/x-yaml";

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = $"/{requestPath}",
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
        });

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = routePrefix;

            foreach (var path in yamlFiles)
            {
                var text = Path.GetRelativePath(openApiPath, path).Replace('\\', '/');
                var name = Path.GetFileNameWithoutExtension(path);
                options.SwaggerEndpoint($"/{requestPath}/{text}", name);
            }

            if (withCredentials)
                options.UseRequestInterceptor("(req) => { req.credentials = 'include'; return req; }");
        });

        app.MapGet("/", () => Results.Redirect($"/{routePrefix}")).AllowAnonymous();
    }
}

namespace Fudie.Security.Extensions;

/// <summary>
/// Extension methods for registering the Fudie JWKS signing key provider.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a JWKS-based signing key provider with caching via <see cref="IMemoryCache"/>.
    /// Also registers <see cref="IJwtValidator"/> if not already registered.
    /// Reads configuration from the <c>Fudie:Security</c> section in appsettings.json.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <c>Fudie:Security</c> section is missing or <c>JwksUrl</c> is not configured.
    /// </exception>
    public static IServiceCollection AddFudieJwksProvider(this IServiceCollection services)
    {
        var configuration = GetConfiguration(services);
        var section = configuration.GetSection(FudieSecurityOptions.SectionName);

        if (!section.Exists())
            throw new InvalidOperationException(
                $"Configuration section '{FudieSecurityOptions.SectionName}' is missing. " +
                $"Add it to appsettings.json.");

        services.Configure<FudieSecurityOptions>(section);

        var jwksUrl = section[nameof(FudieSecurityOptions.JwksUrl)];
        if (string.IsNullOrWhiteSpace(jwksUrl))
            throw new InvalidOperationException(
                $"'{FudieSecurityOptions.SectionName}:{nameof(FudieSecurityOptions.JwksUrl)}' is required.");

        services.AddMemoryCache();

        services.AddRefitClient<IJwksApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<FudieSecurityOptions>>().Value;
                client.BaseAddress = new Uri(opts.JwksUrl);
            });

        services.AddSingleton<ISigningKeyProvider, JwksSigningKeyProvider>();
        services.TryAddSingleton<IJwtValidator, JwtValidator>();

        return services;
    }

    private static IConfiguration GetConfiguration(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
        if (descriptor?.ImplementationInstance is IConfiguration config)
            return config;

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IConfiguration>();
    }
}

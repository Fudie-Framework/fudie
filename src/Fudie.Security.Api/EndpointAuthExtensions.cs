namespace Fudie.Security.Api;

/// <summary>
/// Extension methods for adding authorization metadata to endpoints.
/// </summary>
public static class EndpointAuthExtensions
{
    /// <summary>Marks the endpoint as requiring authentication.</summary>
    public static TBuilder RequireAuthenticated<TBuilder>(
        this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new AuthenticatedRequirement());

    /// <summary>Marks the endpoint as platform-only (requires platform tenant).</summary>
    public static TBuilder RequirePlatform<TBuilder>(
        this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new PlatformRequirement());

    /// <summary>Marks the endpoint as internal (service-to-service only).</summary>
    public static TBuilder RequireInternal<TBuilder>(
        this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new InternalRequirement());

    /// <summary>Marks the endpoint as requiring a specific permission group.</summary>
    public static TBuilder RequireGroup<TBuilder>(
        this TBuilder builder, string group, string description) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new GroupRequirement(group, description));

    /// <summary>Adds a catalog description to the endpoint.</summary>
    public static TBuilder WithDescriptionCatalog<TBuilder>(
        this TBuilder builder, string description) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new CatalogDescription(description));
}

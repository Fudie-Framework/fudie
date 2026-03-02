namespace Fudie.Api;

/// <summary>
/// Provides context for configuring feature pipeline extensions
/// (e.g. authorization, catalog) after endpoints have been mapped.
/// </summary>
public sealed class FeatureBuilder
{
    public IApplicationBuilder App { get; }
    public IEndpointRouteBuilder Routes { get; }
    public IReadOnlyList<FeatureEndpointMapping> EndpointMappings => _mappings.AsReadOnly();

    private readonly List<FeatureEndpointMapping> _mappings = [];

    internal FeatureBuilder(IApplicationBuilder app, IEndpointRouteBuilder routes)
    {
        App = app;
        Routes = routes;
    }

    internal void AddMapping(FeatureEndpointMapping mapping) => _mappings.Add(mapping);
}

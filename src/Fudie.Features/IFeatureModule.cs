namespace Fudie.Features;

/// <summary>
/// Contract for feature endpoint registration. Implementations define
/// their routes in <see cref="AddRoutes"/> and are discovered automatically
/// by <see cref="RouteExtension.MapFeatures"/>.
/// </summary>
public interface IFeatureModule
{
    /// <summary>
    /// Registers the feature's endpoints on the given route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    void AddRoutes(IEndpointRouteBuilder app);
}

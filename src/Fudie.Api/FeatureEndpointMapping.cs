namespace Fudie.Api;

/// <summary>
/// Maps a registered endpoint to the <see cref="IFeatureModule"/> class
/// that created it, preserving the feature namespace for aggregate resolution.
/// </summary>
public record FeatureEndpointMapping(string ClassName, string? FeatureNamespace, Endpoint Endpoint);

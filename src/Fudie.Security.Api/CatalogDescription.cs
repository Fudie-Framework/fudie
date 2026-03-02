namespace Fudie.Security.Api;

/// <summary>
/// Metadata marker that adds a human-readable description to an endpoint in the catalog.
/// </summary>
public class CatalogDescription(string description)
{
    /// <summary>Description text for this endpoint.</summary>
    public string Description { get; } = description;
}

namespace Fudie.Features;

/// <summary>
/// Metadata attribute that provides a description for an endpoint in the catalog.
/// </summary>
/// <param name="description">The endpoint description text.</param>
public class CatalogDescription(string description)
{
    /// <summary>Gets the endpoint description.</summary>
    public string Description { get; } = description;
}

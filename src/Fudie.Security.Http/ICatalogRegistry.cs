namespace Fudie.Security.Http;

/// <summary>
/// Registry that stores endpoint-to-aggregate catalog entries for authorization and discovery.
/// </summary>
public interface ICatalogRegistry
{
    /// <summary>
    /// Registers an endpoint with its associated aggregate description.
    /// </summary>
    void Register(string className, Endpoint endpoint, IAggregateDescription aggregate);

    /// <summary>
    /// Finds an endpoint by its display name.
    /// </summary>
    Endpoint? FindEndpoint(string displayName);

    /// <summary>
    /// Finds the feature class name associated with an endpoint.
    /// </summary>
    string? FindClassName(Endpoint endpoint);

    /// <summary>
    /// Number of registered endpoint entries.
    /// </summary>
    int EndpointMapCount { get; }

    /// <summary>
    /// Returns all catalog entries.
    /// </summary>
    IReadOnlyList<CatalogEntry> GetAll();

    /// <summary>
    /// Returns catalog entries visible to tenants (excludes platform and internal).
    /// </summary>
    IReadOnlyList<CatalogEntry> GetTenant();
}

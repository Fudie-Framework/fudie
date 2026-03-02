namespace Fudie.Features;

/// <summary>
/// Contract for registering and querying endpoint catalog entries.
/// The implementation lives in <c>Fudie.Security</c> (opt-in).
/// </summary>
public interface ICatalogRegistry
{
    /// <summary>Registers an endpoint in the catalog.</summary>
    void Register(string className, Endpoint endpoint, IAggregateDescription aggregate);

    /// <summary>Finds an endpoint by its display name.</summary>
    Endpoint? FindEndpoint(string displayName);

    /// <summary>Finds the class name that registered the given endpoint.</summary>
    string? FindClassName(Endpoint endpoint);

    /// <summary>Gets the total number of mapped endpoints.</summary>
    int EndpointMapCount { get; }

    /// <summary>Gets all catalog entries.</summary>
    IReadOnlyList<CatalogEntry> GetAll();

    /// <summary>Gets catalog entries scoped to the current tenant.</summary>
    IReadOnlyList<CatalogEntry> GetTenant();
}

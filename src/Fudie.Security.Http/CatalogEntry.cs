namespace Fudie.Security.Http;

/// <summary>
/// Immutable entry describing a registered endpoint in the catalog.
/// </summary>
public record CatalogEntry(
    string ClassName,
    string HttpVerb,
    string RoutePattern,
    bool IsAnonymous,
    bool IsAuthenticated,
    bool IsInternal,
    bool IsPlatform,
    string? Description,
    string AggregateId,
    string AggregateDisplayName,
    string? AggregateIcon,
    string AggregateReadDescription,
    string AggregateWriteDescription,
    string Scope,
    string ScopeDescription);

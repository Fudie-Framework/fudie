namespace Fudie.Security.Http;

/// <summary>
/// Singleton registry that builds and stores catalog entries from discovered endpoints.
/// </summary>
[Injectable(ServiceLifetime.Singleton)]
public class CatalogRegistry : ICatalogRegistry
{
    private readonly Dictionary<string, CatalogEntry> _entries = [];
    private readonly Dictionary<string, Endpoint> _endpointMap = [];
    private readonly Dictionary<string, string> _classNameMap = [];

    /// <inheritdoc />
    public void Register(string className, Endpoint endpoint, IAggregateDescription aggregate)
    {
        var displayName = endpoint.DisplayName ?? className;
        var metadata = endpoint.Metadata;

        _endpointMap[displayName] = endpoint;
        _classNameMap[displayName] = className;

        if (metadata.GetMetadata<ExcludeFromDescriptionAttribute>() is not null)
            return;

        var httpMethod = ResolveHttpMethod(metadata);
        var groupRequirement = metadata.GetMetadata<GroupRequirement>();

        _entries[displayName] = new CatalogEntry(
            ClassName: className,
            HttpVerb: httpMethod,
            RoutePattern: ResolveRoutePattern(endpoint),
            IsAnonymous: metadata.GetMetadata<AllowAnonymousAttribute>() is not null,
            IsAuthenticated: metadata.GetMetadata<AuthenticatedRequirement>() is not null,
            IsInternal: metadata.GetMetadata<InternalRequirement>() is not null,
            IsPlatform: metadata.GetMetadata<PlatformRequirement>() is not null,
            Description: metadata.GetMetadata<CatalogDescription>()?.Description,
            AggregateId: aggregate.Id,
            AggregateDisplayName: aggregate.DisplayName,
            AggregateIcon: aggregate.Icon,
            AggregateReadDescription: aggregate.ReadDescription,
            AggregateWriteDescription: aggregate.WriteDescription,
            Scope: ResolveScope(groupRequirement, httpMethod, aggregate),
            ScopeDescription: ResolveScopeDescription(groupRequirement, httpMethod, aggregate));
    }

    private static string ResolveHttpMethod(EndpointMetadataCollection metadata)
        => metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";

    private static string ResolveRoutePattern(Endpoint endpoint)
        => endpoint is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? ""
            : "";

    private static string ResolveScope(
        GroupRequirement? groupRequirement, string httpMethod, IAggregateDescription aggregate)
        => groupRequirement?.Group
            ?? (httpMethod is "GET" ? $"{aggregate.Id}:read" : $"{aggregate.Id}:write");

    private static string ResolveScopeDescription(
        GroupRequirement? groupRequirement, string httpMethod, IAggregateDescription aggregate)
        => groupRequirement is not null
            ? (groupRequirement.Description ?? groupRequirement.Group)
            : (httpMethod is "GET" ? aggregate.ReadDescription : aggregate.WriteDescription);

    /// <inheritdoc />
    public Endpoint? FindEndpoint(string displayName)
        => _endpointMap.GetValueOrDefault(displayName);

    /// <inheritdoc />
    public string? FindClassName(Endpoint endpoint)
        => endpoint.DisplayName is not null
            && _classNameMap.TryGetValue(endpoint.DisplayName, out var name) ? name : null;

    /// <inheritdoc />
    public int EndpointMapCount => _entries.Count;

    /// <inheritdoc />
    public IReadOnlyList<CatalogEntry> GetAll()
        => _entries.Values.ToList().AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<CatalogEntry> GetTenant()
        => _entries.Values.Where(e => !e.IsPlatform && !e.IsInternal).ToList().AsReadOnly();
}

using System.Reflection;

namespace Fudie.Security.Api;

/// <summary>
/// Extension methods for enabling Fudie authorization on a <see cref="FeatureBuilder"/>.
/// </summary>
public static class FudieAuthorizationExtensions
{
    /// <summary>
    /// Enables Fudie authorization middleware, registers endpoints in the catalog,
    /// and maps the <c>GET /catalog</c> discovery endpoint.
    /// </summary>
    public static FeatureBuilder UseFudieAuthorization(this FeatureBuilder builder)
    {
        var aggregateDescriptions = BuildAggregateDescriptions();
        var catalog = builder.Routes.ServiceProvider.GetRequiredService<ICatalogRegistry>();

        foreach (var mapping in builder.EndpointMappings)
        {
            var aggregate = ResolveAggregate(mapping.ClassName, mapping.FeatureNamespace, aggregateDescriptions);
            catalog.Register(mapping.ClassName, mapping.Endpoint, aggregate);
        }

        builder.App.UseMiddleware<FudieAuthorizationMiddleware>();
        builder.Routes.MapCatalog();

        return builder;
    }

    internal static Dictionary<string, IAggregateDescription> BuildAggregateDescriptions()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .ToList();

        return assemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic
                && t.IsAssignableTo(typeof(IAggregateDescription))
                && t.GetConstructor(Type.EmptyTypes) is not null)
            .Select(t => (Type: t, Instance: (IAggregateDescription)Activator.CreateInstance(t)!))
            .ToDictionary(x => x.Type.Namespace!, x => x.Instance);
    }

    internal static string ExtractAggregateNamespace(string featureNamespace)
    {
        var lastDot = featureNamespace.LastIndexOf('.');
        if (lastDot <= 0)
            throw new InvalidOperationException(
                $"Cannot extract aggregate namespace from '{featureNamespace}'. " +
                $"Expected pattern: {{Project}}.Features.{{Feature}}.Api.{{Aggregate}}Aggregate.{{Commands|Queries}}");

        return featureNamespace[..lastDot];
    }

    internal static IAggregateDescription ResolveAggregate(
        string className,
        string? featureNamespace,
        Dictionary<string, IAggregateDescription> aggregateDescriptions)
    {
        if (featureNamespace is null)
            throw new InvalidOperationException(
                $"IFeatureModule '{className}' has no namespace.");

        var aggregateNamespace = ExtractAggregateNamespace(featureNamespace);

        if (aggregateDescriptions.TryGetValue(aggregateNamespace, out var description))
            return description;

        throw new InvalidOperationException(
            $"No IAggregateDescription found for namespace '{aggregateNamespace}'. " +
            $"Feature: {className}. " +
            $"Expected an IAggregateDescription implementation in namespace '{aggregateNamespace}'. " +
            $"Create a class implementing IAggregateDescription in the aggregate folder.");
    }

    private static Type[] SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException) { return []; }
    }
}

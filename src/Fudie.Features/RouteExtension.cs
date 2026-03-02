namespace Fudie.Features;

/// <summary>
/// Extension methods for <see cref="IEndpointRouteBuilder"/> that discover
/// <see cref="IFeatureModule"/> implementations and map their endpoints.
/// </summary>
public static class RouteExtension
{
    /// <summary>
    /// Discovers all <see cref="IFeatureModule"/> implementations, calls
    /// <see cref="IFeatureModule.AddRoutes"/>, and optionally registers
    /// endpoints in <see cref="ICatalogRegistry"/> when available.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFeatures(this IEndpointRouteBuilder builder)
    {
        var assemblies = DiscoverAssemblies();
        var allTypes = DiscoverTypes(assemblies);
        var aggregateDescriptions = BuildAggregateDescriptions(allTypes);
        var features = DiscoverFeatures(allTypes);
        var catalog = builder.ServiceProvider.GetService<ICatalogRegistry>();

        foreach (var feature in features)
        {
            RegisterFeature(builder, feature, aggregateDescriptions, catalog);
        }
    }

    private static List<Assembly> DiscoverAssemblies()
    {
        var interfaceAssembly = typeof(IFeatureModule).Assembly;

        return [.. AppDomain.CurrentDomain.GetAssemblies().Where(a => ReferencesFeatureAssembly(a, interfaceAssembly))];
    }

    private static bool ReferencesFeatureAssembly(Assembly assembly, Assembly interfaceAssembly)
    {
        return !assembly.IsDynamic &&
               (assembly == interfaceAssembly ||
                assembly.GetReferencedAssemblies()
                    .Any(ra => ra.Name == interfaceAssembly.GetName().Name));
    }

    private static List<Type> DiscoverTypes(List<Assembly> assemblies)
    {
        return [.. assemblies.SelectMany(SafeGetTypes)];
    }
    
    private static Type[] SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException)
        {
            return [];
        }
    }

    private static Dictionary<string, IAggregateDescription> BuildAggregateDescriptions(
        List<Type> allTypes)
    {
        return allTypes
            .Where(IsInstantiableAggregateDescription)
            .Select(t => (Type: t, Instance: (IAggregateDescription)Activator.CreateInstance(t)!))
            .ToDictionary(x => x.Type.Namespace!, x => x.Instance);
    }

    private static bool IsInstantiableAggregateDescription(Type t)
    {
        return t.IsClass && !t.IsAbstract && t.IsPublic
            && t.IsAssignableTo(typeof(IAggregateDescription))
            && t.GetConstructor(Type.EmptyTypes) is not null;
    }

    private static IEnumerable<IFeatureModule> DiscoverFeatures(List<Type> allTypes)
    {
        return allTypes
            .Where(IsInstantiableFeature)
            .Select(Activator.CreateInstance)
            .Cast<IFeatureModule>();
    }

    private static bool IsInstantiableFeature(Type t)
    {
        return t.IsClass && !t.IsAbstract && t.IsPublic
            && t.IsAssignableTo(typeof(IFeatureModule));
    }

    private static void RegisterFeature(
        IEndpointRouteBuilder builder,
        IFeatureModule feature,
        Dictionary<string, IAggregateDescription> aggregateDescriptions,
        ICatalogRegistry? catalog)
    {
        var aggregate = ResolveAggregate(feature, aggregateDescriptions);

        var countBefore = builder.DataSources
            .SelectMany(ds => ds.Endpoints).Count();

        feature.AddRoutes(builder);

        RegisterNewEndpoints(builder, feature.GetType().Name, aggregate, catalog, countBefore);
    }

    private static void RegisterNewEndpoints(
        IEndpointRouteBuilder builder,
        string className,
        IAggregateDescription aggregate,
        ICatalogRegistry? catalog,
        int countBefore)
    {
        if (catalog is null) return;

        foreach (var endpoint in builder.DataSources
            .SelectMany(ds => ds.Endpoints).Skip(countBefore))
        {
            catalog.Register(className, endpoint, aggregate);
        }
    }

    private static string ExtractAggregateNamespace(string featureNamespace)
    {
        var lastDot = featureNamespace.LastIndexOf('.');
        if (lastDot <= 0)
            throw new InvalidOperationException(
                $"Cannot extract aggregate namespace from '{featureNamespace}'. " +
                $"Expected pattern: {{Project}}.Features.{{Feature}}.Api.{{Aggregate}}Aggregate.{{Commands|Queries}}");

        return featureNamespace[..lastDot];
    }

    private static IAggregateDescription ResolveAggregate(
        IFeatureModule feature,
        Dictionary<string, IAggregateDescription> aggregateDescriptions)
    {
        var featureNamespace = feature.GetType().Namespace
            ?? throw new InvalidOperationException(
                $"IFeatureModule '{feature.GetType().Name}' has no namespace.");

        var aggregateNamespace = ExtractAggregateNamespace(featureNamespace);

        if (aggregateDescriptions.TryGetValue(aggregateNamespace, out var description))
            return description;

        throw new InvalidOperationException(
            $"No IAggregateDescription found for namespace '{aggregateNamespace}'. " +
            $"Feature: {feature.GetType().FullName}. " +
            $"Expected an IAggregateDescription implementation in namespace '{aggregateNamespace}'. " +
            $"Create a class implementing IAggregateDescription in the aggregate folder.");
    }
}

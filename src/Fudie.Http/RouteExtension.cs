namespace Fudie.Http;

/// <summary>
/// Extension methods for <see cref="IEndpointRouteBuilder"/> that discover
/// <see cref="IFeatureModule"/> implementations and map their endpoints.
/// </summary>
public static class RouteExtension
{
    /// <summary>
    /// Discovers all <see cref="IFeatureModule"/> implementations, calls
    /// <see cref="IFeatureModule.AddRoutes"/>, and optionally invokes a
    /// configuration callback for pipeline extensions (e.g. authorization).
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="configure">Optional callback to configure extensions after endpoints are mapped.</param>
    public static void MapFeatures(
        this IEndpointRouteBuilder builder,
        Action<FeatureBuilder>? configure = null)
    {
        var assemblies = DiscoverAssemblies();
        var allTypes = DiscoverTypes(assemblies);
        var features = DiscoverFeatures(allTypes);

        var featureBuilder = new FeatureBuilder(
            builder as IApplicationBuilder
                ?? throw new InvalidOperationException(
                    "MapFeatures requires an IApplicationBuilder (e.g. WebApplication)."),
            builder);

        foreach (var feature in features)
        {
            RegisterFeature(builder, feature, featureBuilder);
        }

        configure?.Invoke(featureBuilder);
    }

    private static List<Assembly> DiscoverAssemblies()
    {
        var interfaceAssembly = typeof(IFeatureModule).Assembly;

        return [.. AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => ReferencesFeatureAssembly(a, interfaceAssembly))];
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
        FeatureBuilder featureBuilder)
    {
        var countBefore = builder.DataSources
            .SelectMany(ds => ds.Endpoints).Count();

        feature.AddRoutes(builder);

        var className = feature.GetType().Name;
        var featureNamespace = feature.GetType().Namespace;

        foreach (var endpoint in builder.DataSources
            .SelectMany(ds => ds.Endpoints).Skip(countBefore))
        {
            featureBuilder.AddMapping(
                new FeatureEndpointMapping(className, featureNamespace, endpoint));
        }
    }
}

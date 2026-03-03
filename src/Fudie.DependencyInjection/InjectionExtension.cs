using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fudie.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register classes
/// decorated with <see cref="InjectableAttribute"/>.
/// </summary>
public static class InjectionExtension
{
    /// <summary>
    /// Scans assemblies for classes decorated with <see cref="InjectableAttribute"/>
    /// and registers them in the DI container with the configured lifetime.
    /// When no assemblies are provided, the calling assembly and any assembly referencing
    /// <see cref="InjectableAttribute"/> are scanned automatically.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Optional assemblies to scan. When empty, automatic discovery is used.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInjectables(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = DiscoverRelevantAssemblies();
        }

        var injectableTypes = DiscoverInjectableTypes(assemblies);

        foreach (var implementationType in injectableTypes)
        {
            RegisterInjectableType(services, implementationType);
        }

        return services;
    }

    /// <summary>
    /// Registers the interfaces implemented by a type that is already registered in the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the interface registrations.</param>
    /// <typeparam name="TImplementation">The implementation type already registered.</typeparam>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInterfacesFor<TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);

        if (!IsServiceRegistered(services, implementationType))
        {
            throw new InvalidOperationException(
                $"Cannot register interfaces for {implementationType.Name} because it's not registered in the container. " +
                $"Please register {implementationType.Name} before calling AddInterfacesFor.");
        }

        var topLevelInterfaces = GetTopLevelInterfaces(implementationType);

        foreach (var interfaceType in topLevelInterfaces)
        {
            RegisterForwardingIfNeeded(services, interfaceType, sp => sp.GetRequiredService<TImplementation>(), lifetime);
        }

        return services;
    }

    private static IEnumerable<Type> DiscoverInjectableTypes(Assembly[] assemblies)
    {
        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttributes<InjectableAttribute>().Any()
                        && type.IsClass && !type.IsAbstract);
    }

    private static void RegisterInjectableType(IServiceCollection services, Type implementationType)
    {
        var attributes = implementationType.GetCustomAttributes<InjectableAttribute>().ToList();
        var lifetime = attributes.First().Lifetime;

        if (!IsServiceRegistered(services, implementationType))
        {
            RegisterService(services, implementationType, implementationType, lifetime);
        }

        var explicitServiceTypes = attributes
            .Where(a => a.ServiceType is not null)
            .Select(a => a.ServiceType!)
            .ToHashSet();

        if (explicitServiceTypes.Count > 0)
        {
            RegisterExplicitServiceTypes(services, implementationType, explicitServiceTypes, lifetime);
        }
        else
        {
            RegisterDiscoveredInterfaces(services, implementationType, lifetime);
        }
    }

    private static void RegisterExplicitServiceTypes(
        IServiceCollection services,
        Type implementationType,
        HashSet<Type> serviceTypes,
        ServiceLifetime lifetime)
    {
        foreach (var serviceType in serviceTypes)
        {
            RegisterForwardingIfNeeded(services, serviceType, sp => sp.GetRequiredService(implementationType), lifetime);
        }
    }

    private static void RegisterDiscoveredInterfaces(
        IServiceCollection services,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        var topLevelInterfaces = GetTopLevelInterfaces(implementationType);

        foreach (var interfaceType in topLevelInterfaces)
        {
            RegisterForwardingIfNeeded(services, interfaceType, sp => sp.GetRequiredService(implementationType), lifetime);
        }
    }

    private static void RegisterForwardingIfNeeded(
        IServiceCollection services,
        Type serviceType,
        Func<IServiceProvider, object> factory,
        ServiceLifetime lifetime)
    {
        if (!IsServiceRegistered(services, serviceType))
        {
            RegisterServiceFromFactory(services, serviceType, factory, lifetime);
        }
    }

    private static HashSet<Type> GetTopLevelInterfaces(Type type)
    {
        var allInterfaces = type.GetInterfaces();
        var topLevelInterfaces = new HashSet<Type>();

        foreach (var interfaceType in allInterfaces)
        {
            bool isInheritedByOther = allInterfaces.Any(other =>
                other != interfaceType && other.GetInterfaces().Contains(interfaceType));

            if (!isInheritedByOther)
            {
                topLevelInterfaces.Add(interfaceType);
            }
        }

        return topLevelInterfaces;
    }

    private static Assembly[] DiscoverRelevantAssemblies()
    {
        var attributeAssemblyName = typeof(InjectableAttribute).Assembly.GetName().Name;

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic
                && a.GetReferencedAssemblies().Any(r => r.Name == attributeAssemblyName))
            .ToArray();
    }

    private static bool IsServiceRegistered(IServiceCollection services, Type serviceType)
    {
        return services.Any(descriptor => descriptor.ServiceType == serviceType);
    }

    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        var serviceDescriptor = CreateServiceDescriptor(serviceType, implementationType, lifetime);
        services.TryAdd(serviceDescriptor);
    }

    private static void RegisterServiceFromFactory(
        IServiceCollection services,
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime)
    {
        var serviceDescriptor = CreateServiceDescriptor(serviceType, implementationFactory, lifetime);
        services.TryAdd(serviceDescriptor);
    }

    private static ServiceDescriptor CreateServiceDescriptor(
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => ServiceDescriptor.Transient(serviceType, implementationType),
            ServiceLifetime.Scoped => ServiceDescriptor.Scoped(serviceType, implementationType),
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton(serviceType, implementationType),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime")
        };
    }

    private static ServiceDescriptor CreateServiceDescriptor(
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => ServiceDescriptor.Transient(serviceType, implementationFactory),
            ServiceLifetime.Scoped => ServiceDescriptor.Scoped(serviceType, implementationFactory),
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton(serviceType, implementationFactory),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime")
        };
    }
}

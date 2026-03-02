namespace Fudie.DependencyInjection;

/// <summary>
/// Marks a class for automatic dependency injection registration.
/// The DI scanner discovers all types decorated with this attribute and registers them
/// with the configured <see cref="ServiceLifetime"/> and optional service type.
/// </summary>
/// <param name="lifetime">The service lifetime (defaults to <see cref="ServiceLifetime.Scoped"/>).</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : Attribute
{
    /// <summary>
    /// Gets the service lifetime used when registering this type in the DI container.
    /// </summary>
    public ServiceLifetime Lifetime { get; init; } = lifetime;

    /// <summary>
    /// Gets or sets an explicit service type to register. When <c>null</c>, the class is
    /// registered by its top-level interfaces automatically.
    /// </summary>
    public Type? ServiceType { get; init; }
}

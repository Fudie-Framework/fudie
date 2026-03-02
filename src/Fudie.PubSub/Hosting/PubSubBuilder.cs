namespace Fudie.PubSub.Hosting;

/// <summary>
/// Builder for configuring pub/sub messaging services.
/// </summary>
/// <param name="services">The service collection being configured.</param>
public class PubSubBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; } = services;
}

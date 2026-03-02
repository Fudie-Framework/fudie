namespace Fudie.PubSub.Hosting;

/// <summary>
/// Extension methods for registering pub/sub messaging services in the dependency injection container.
/// </summary>
public static class MessagingServiceExtensions
{
    /// <summary>
    /// Adds pub/sub messaging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">An action to configure the <see cref="PubSubBuilder"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPubSubMessaging(
        this IServiceCollection services,
        Action<PubSubBuilder> configure)
    {
        var builder = new PubSubBuilder(services);
        configure(builder);

        services.AddScoped<MessageContext>();
        services.AddScoped<IMessageContext>(sp => sp.GetRequiredService<MessageContext>());
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<MessageHost>();

        return services;
    }
}

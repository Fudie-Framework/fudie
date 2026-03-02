namespace Fudie.PubSub.UnitTests.Hosting;

public class MessagingServiceExtensionsTests
{
    [Fact]
    public void AddPubSubMessaging_RegistersAllRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPubSubClient>(new StubPubSubClient());

        services.AddPubSubMessaging(builder =>
        {
            builder.Services.Should().BeSameAs(services);
        });

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<MessageContext>().Should().NotBeNull();
        provider.GetRequiredService<IMessageContext>().Should().NotBeNull();
        provider.GetRequiredService<IMessagePublisher>().Should().NotBeNull();
        provider.GetRequiredService<MessageHost>().Should().NotBeNull();
    }

    [Fact]
    public void AddPubSubMessaging_MessageContextAndIMessageContext_AreSameInstance()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPubSubClient>(new StubPubSubClient());
        services.AddPubSubMessaging(_ => { });

        using var scope = services.BuildServiceProvider().CreateScope();

        var concrete = scope.ServiceProvider.GetRequiredService<MessageContext>();
        var iface = scope.ServiceProvider.GetRequiredService<IMessageContext>();

        iface.Should().BeSameAs(concrete);
    }

    [Fact]
    public void AddPubSubMessaging_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddPubSubMessaging(_ => { });

        result.Should().BeSameAs(services);
    }
}

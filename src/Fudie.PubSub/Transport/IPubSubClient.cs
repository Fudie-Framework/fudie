namespace Fudie.PubSub.Transport;

/// <summary>
/// Unified pub/sub client combining topic administration, subscription administration,
/// publishing, and subscribing capabilities.
/// </summary>
public interface IPubSubClient : ITopicAdmin, ISubscriptionAdmin, IPublisher, ISubscriber;

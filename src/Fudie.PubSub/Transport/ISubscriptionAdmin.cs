namespace Fudie.PubSub.Transport;

/// <summary>
/// Provides administrative operations for managing subscriptions.
/// </summary>
public interface ISubscriptionAdmin
{
    /// <summary>
    /// Creates a new subscription linked to the specified topic.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to create.</param>
    /// <param name="topicId">The topic identifier to subscribe to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateSubscriptionAsync(string subscriptionId, string topicId);

    /// <summary>
    /// Deletes an existing subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Checks whether a subscription exists.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to check.</param>
    /// <returns><c>true</c> if the subscription exists; otherwise, <c>false</c>.</returns>
    Task<bool> SubscriptionExistsAsync(string subscriptionId);
}

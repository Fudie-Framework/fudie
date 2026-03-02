namespace Fudie.PubSub.Transport;

/// <summary>
/// Provides administrative operations for managing topics.
/// </summary>
public interface ITopicAdmin
{
    /// <summary>
    /// Creates a new topic.
    /// </summary>
    /// <param name="topicId">The topic identifier to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateTopicAsync(string topicId);

    /// <summary>
    /// Deletes an existing topic.
    /// </summary>
    /// <param name="topicId">The topic identifier to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteTopicAsync(string topicId);

    /// <summary>
    /// Checks whether a topic exists.
    /// </summary>
    /// <param name="topicId">The topic identifier to check.</param>
    /// <returns><c>true</c> if the topic exists; otherwise, <c>false</c>.</returns>
    Task<bool> TopicExistsAsync(string topicId);
}

namespace Fudie.PubSub.UnitTests;

public class PubSubClientValidationTests
{
    private readonly IPubSubClient _client = new StubPubSubClient();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateTopicAsync_ThrowsOnNullOrEmpty(string? topicId)
    {
        var act = () => _client.CreateTopicAsync(topicId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteTopicAsync_ThrowsOnNullOrEmpty(string? topicId)
    {
        var act = () => _client.DeleteTopicAsync(topicId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task TopicExistsAsync_ThrowsOnNullOrEmpty(string? topicId)
    {
        var act = () => _client.TopicExistsAsync(topicId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateSubscriptionAsync_ThrowsOnNullOrEmptySubscriptionId(string? subscriptionId)
    {
        var act = () => _client.CreateSubscriptionAsync(subscriptionId!, "topic");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateSubscriptionAsync_ThrowsOnNullOrEmptyTopicId(string? topicId)
    {
        var act = () => _client.CreateSubscriptionAsync("sub", topicId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteSubscriptionAsync_ThrowsOnNullOrEmpty(string? subscriptionId)
    {
        var act = () => _client.DeleteSubscriptionAsync(subscriptionId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SubscriptionExistsAsync_ThrowsOnNullOrEmpty(string? subscriptionId)
    {
        var act = () => _client.SubscriptionExistsAsync(subscriptionId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PublishAsync_ThrowsOnNullOrEmptyTopicId(string? topicId)
    {
        var act = () => _client.PublishAsync(topicId!, new SampleMessage("Pedro", 30));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_ThrowsOnNullMessage()
    {
        var act = () => _client.PublishAsync<SampleMessage>("topic", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SubscribeAsync_ThrowsOnNullOrEmptySubscriptionId(string? subscriptionId)
    {
        var act = () => _client.SubscribeAsync<SampleMessage>(subscriptionId!, (_, _) => Task.CompletedTask);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SubscribeAsync_ThrowsOnNullHandler()
    {
        var act = () => _client.SubscribeAsync<SampleMessage>("sub", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateTopicAsync_WithValidId_Completes()
    {
        await _client.CreateTopicAsync("valid-topic");
    }

    [Fact]
    public async Task DeleteTopicAsync_WithValidId_Completes()
    {
        await _client.DeleteTopicAsync("valid-topic");
    }

    [Fact]
    public async Task TopicExistsAsync_WithValidId_ReturnsFalse()
    {
        var result = await _client.TopicExistsAsync("valid-topic");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WithValidIds_Completes()
    {
        await _client.CreateSubscriptionAsync("valid-sub", "valid-topic");
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_WithValidId_Completes()
    {
        await _client.DeleteSubscriptionAsync("valid-sub");
    }

    [Fact]
    public async Task SubscriptionExistsAsync_WithValidId_ReturnsFalse()
    {
        var result = await _client.SubscriptionExistsAsync("valid-sub");

        result.Should().BeFalse();
    }
}

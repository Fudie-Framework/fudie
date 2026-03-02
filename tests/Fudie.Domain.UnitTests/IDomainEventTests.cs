namespace Fudie.Domain.UnitTests;

public class IDomainEventTests
{
    [Fact]
    public void IDomainEvent_ShouldBeAnInterface()
    {
        typeof(IDomainEvent).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IDomainEvent_ShouldDeclareExactlyOneProperty()
    {
        typeof(IDomainEvent).GetProperties().Should().HaveCount(1);
    }

    [Fact]
    public void IDomainEvent_ShouldDeclareOccurredAtGetter()
    {
        var property = typeof(IDomainEvent).GetProperty(nameof(IDomainEvent.OccurredAt));

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(DateTime));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IDomainEvent_ShouldDeclareNoMethods()
    {
        var methods = typeof(IDomainEvent).GetMethods()
            .Where(m => !m.IsSpecialName)
            .ToArray();

        methods.Should().BeEmpty();
    }
}

namespace Fudie.Domain.UnitTests;

public class AggregateRootTests
{
    private record TestEvent(DateTime OccurredAt) : IDomainEvent;

    private class TestAggregate(Guid id) : AggregateRoot<Guid>(id);

    [Fact]
    public void NewAggregate_ShouldHaveNoDomainEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestEvent(DateTime.UtcNow);

        aggregate.AddDomainEvent(domainEvent);

        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeSameAs(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_MultipleTimes_ShouldAccumulateEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var event1 = new TestEvent(DateTime.UtcNow);
        var event2 = new TestEvent(DateTime.UtcNow);

        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        aggregate.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestEvent(DateTime.UtcNow));
        aggregate.AddDomainEvent(new TestEvent(DateTime.UtcNow));

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_ShouldImplementIHasDomainEvents()
    {
        IHasDomainEvents aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.Should().BeAssignableTo<IHasDomainEvents>();
    }

    [Fact]
    public void AggregateRoot_ShouldInheritFromEntity()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.Should().BeAssignableTo<Entity<Guid>>();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());

        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }
}

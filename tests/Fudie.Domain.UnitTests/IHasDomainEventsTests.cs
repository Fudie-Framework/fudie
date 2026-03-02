namespace Fudie.Domain.UnitTests;

public class IHasDomainEventsTests
{
    [Fact]
    public void IHasDomainEvents_ShouldBeAnInterface()
    {
        typeof(IHasDomainEvents).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IHasDomainEvents_ShouldDeclareExactlyOneProperty()
    {
        typeof(IHasDomainEvents).GetProperties().Should().HaveCount(1);
    }

    [Fact]
    public void IHasDomainEvents_ShouldDeclareDomainEventsGetter()
    {
        var property = typeof(IHasDomainEvents).GetProperty(nameof(IHasDomainEvents.DomainEvents));

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(IReadOnlyCollection<IDomainEvent>));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IHasDomainEvents_ShouldDeclareExactlyOneMethod()
    {
        var methods = typeof(IHasDomainEvents).GetMethods()
            .Where(m => !m.IsSpecialName)
            .ToArray();

        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IHasDomainEvents_ShouldDeclareClearDomainEventsMethod()
    {
        var method = typeof(IHasDomainEvents).GetMethod(nameof(IHasDomainEvents.ClearDomainEvents));

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().BeEmpty();
    }
}

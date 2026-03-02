namespace Fudie.Domain.UnitTests;

public class IEntityTests
{
    [Fact]
    public void IEntity_ShouldBeAnInterface()
    {
        typeof(IEntity).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IEntity_ShouldDeclareNoMembers()
    {
        typeof(IEntity).GetMembers().Should().BeEmpty();
    }

    [Fact]
    public void IEntityOfTId_ShouldBeAnInterface()
    {
        typeof(IEntity<>).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IEntityOfTId_ShouldInheritFromIEntity()
    {
        typeof(IEntity<Guid>).Should().BeAssignableTo<IEntity>();
    }

    [Fact]
    public void IEntityOfTId_ShouldHaveOneGenericParameter()
    {
        typeof(IEntity<>).GetGenericArguments().Should().HaveCount(1);
    }

    [Fact]
    public void IEntityOfTId_ShouldDeclareExactlyOneProperty()
    {
        typeof(IEntity<Guid>).GetProperties().Should().HaveCount(1);
    }

    [Fact]
    public void IEntityOfTId_ShouldDeclareIdGetter()
    {
        var property = typeof(IEntity<Guid>).GetProperty(nameof(IEntity<Guid>.Id));

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IEntityOfTId_ShouldDeclareNoMethods()
    {
        var methods = typeof(IEntity<Guid>).GetMethods()
            .Where(m => !m.IsSpecialName)
            .ToArray();

        methods.Should().BeEmpty();
    }
}

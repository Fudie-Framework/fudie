namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IChangeTrackerTests
{
    private static readonly Type Type = typeof(IChangeTracker);

    [Fact]
    public void IChangeTracker_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IChangeTracker_ShouldDeclareExactlyOneMethod()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IChangeTracker_ShouldDeclareEntryMethod()
    {
        var method = Type.GetMethod("Entry");

        method.Should().NotBeNull();
        method!.IsGenericMethod.Should().BeTrue();
        method.GetGenericArguments().Should().HaveCount(1);
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(EntityEntry<>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("entity");
    }
}

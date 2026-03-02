namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IUpdateTests
{
    private static readonly Type Type = typeof(IUpdate<,>);

    [Fact]
    public void IUpdate_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IUpdate_ShouldHaveExactlyTwoGenericParameters()
    {
        Type.GetGenericArguments().Should().HaveCount(2);
    }

    [Fact]
    public void IUpdate_ShouldDeclareNoOwnMembers()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().BeEmpty();
    }

    [Fact]
    public void IUpdate_ShouldInheritFromIGet()
    {
        var interfaces = Type.GetInterfaces();
        interfaces.Should().ContainSingle(i => i.GetGenericTypeDefinition() == typeof(IGet<,>));
    }
}

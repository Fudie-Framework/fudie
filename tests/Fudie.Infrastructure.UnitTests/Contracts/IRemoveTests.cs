namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IRemoveTests
{
    private static readonly Type Type = typeof(IRemove<,>);

    [Fact]
    public void IRemove_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IRemove_ShouldHaveExactlyTwoGenericParameters()
    {
        Type.GetGenericArguments().Should().HaveCount(2);
    }

    [Fact]
    public void IRemove_ShouldDeclareExactlyOneMember()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IRemove_ShouldDeclareRemoveMethod()
    {
        var method = Type.GetMethod("Remove");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("entity");
    }

    [Fact]
    public void IRemove_ShouldInheritFromIGet()
    {
        var interfaces = Type.GetInterfaces();
        interfaces.Should().ContainSingle(i => i.GetGenericTypeDefinition() == typeof(IGet<,>));
    }
}

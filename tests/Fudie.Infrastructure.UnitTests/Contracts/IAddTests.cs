namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IAddTests
{
    private static readonly Type Type = typeof(IAdd<>);

    [Fact]
    public void IAdd_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IAdd_ShouldHaveExactlyOneGenericParameter()
    {
        Type.GetGenericArguments().Should().HaveCount(1);
    }

    [Fact]
    public void IAdd_ShouldDeclareExactlyOneMember()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IAdd_ShouldDeclareAddMethod()
    {
        var method = Type.GetMethod("Add");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("entity");
    }
}

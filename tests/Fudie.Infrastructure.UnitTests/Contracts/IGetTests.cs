namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IGetTests
{
    private static readonly Type Type = typeof(IGet<,>);

    [Fact]
    public void IGet_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IGet_ShouldHaveExactlyTwoGenericParameters()
    {
        Type.GetGenericArguments().Should().HaveCount(2);
    }

    [Fact]
    public void IGet_ShouldDeclareExactlyOneMember()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IGet_ShouldDeclareGetMethod()
    {
        var method = Type.GetMethod("Get");

        method.Should().NotBeNull();
        method!.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("id");
    }
}

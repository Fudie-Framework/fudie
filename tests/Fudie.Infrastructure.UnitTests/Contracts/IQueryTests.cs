namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IQueryTests
{
    private static readonly Type Type = typeof(IQuery);

    [Fact]
    public void IQuery_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IQuery_ShouldDeclareExactlyOneMember()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IQuery_ShouldDeclareQueryMethod()
    {
        var method = Type.GetMethod("Query");

        method.Should().NotBeNull();
        method!.IsGenericMethod.Should().BeTrue();
        method.GetGenericArguments().Should().HaveCount(1);
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(IQueryable<>));
        method.GetParameters().Should().BeEmpty();
    }
}

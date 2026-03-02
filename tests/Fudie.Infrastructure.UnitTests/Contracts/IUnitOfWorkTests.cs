namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IUnitOfWorkTests
{
    private static readonly Type Type = typeof(IUnitOfWork);

    [Fact]
    public void IUnitOfWork_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IUnitOfWork_ShouldDeclareExactlyTwoMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(2);
    }

    [Fact]
    public void IUnitOfWork_ShouldDeclareSaveChangesMethod()
    {
        var method = Type.GetMethod("SaveChanges", Type.EmptyTypes);

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(int));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void IUnitOfWork_ShouldDeclareSaveChangesAsyncMethod()
    {
        var method = Type.GetMethod("SaveChangesAsync");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<int>));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("cancellationToken");
        method.GetParameters()[0].ParameterType.Should().Be(typeof(CancellationToken));
    }
}

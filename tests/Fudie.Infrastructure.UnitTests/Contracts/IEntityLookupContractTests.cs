namespace Fudie.Infrastructure.UnitTests.Contracts;

public class IEntityLookupContractTests
{
    private static readonly Type Type = typeof(IEntityLookup);

    [Fact]
    public void IEntityLookup_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IEntityLookup_ShouldDeclareExactlyTwoMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(2);
    }

    [Fact]
    public void IEntityLookup_ShouldDeclareSetMethod()
    {
        var method = Type.GetMethod("Set");

        method.Should().NotBeNull();
        method!.IsGenericMethod.Should().BeTrue();
        method.GetGenericArguments().Should().HaveCount(1);
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(DbSet<>));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void IEntityLookup_ShouldDeclareGetRequiredAsyncMethod()
    {
        var method = Type.GetMethod("GetRequiredAsync");

        method.Should().NotBeNull();
        method!.IsGenericMethod.Should().BeTrue();
        method.GetGenericArguments().Should().HaveCount(2);
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>));
        method.GetParameters().Should().HaveCount(4);
        method.GetParameters()[0].Name.Should().Be("id");
        method.GetParameters()[1].Name.Should().Be("tracking");
        method.GetParameters()[2].Name.Should().Be("cancellationToken");
        method.GetParameters()[3].Name.Should().Be("includeProperties");
    }
}

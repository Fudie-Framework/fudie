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
    public void IEntityLookup_ShouldDeclareExactlyThreeMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        methods.Should().HaveCount(3);
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
    public void IEntityLookup_ShouldDeclareTwoGetRequiredAsyncOverloads()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == "GetRequiredAsync")
            .OrderBy(m => m.GetParameters().Length)
            .ToArray();

        methods.Should().HaveCount(2);

        // Nullable overload (2 params)
        var nullable = methods[0];
        nullable.IsGenericMethod.Should().BeTrue();
        nullable.GetGenericArguments().Should().HaveCount(2);
        nullable.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>));
        nullable.GetParameters().Should().HaveCount(2);
        nullable.GetParameters()[0].Name.Should().Be("id");
        nullable.GetParameters()[1].Name.Should().Be("tracking");

        // Full overload (4 params)
        var full = methods[1];
        full.IsGenericMethod.Should().BeTrue();
        full.GetGenericArguments().Should().HaveCount(2);
        full.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>));
        full.GetParameters().Should().HaveCount(4);
        full.GetParameters()[0].Name.Should().Be("id");
        full.GetParameters()[1].Name.Should().Be("tracking");
        full.GetParameters()[2].Name.Should().Be("cancellationToken");
        full.GetParameters()[3].Name.Should().Be("includeProperties");
    }
}

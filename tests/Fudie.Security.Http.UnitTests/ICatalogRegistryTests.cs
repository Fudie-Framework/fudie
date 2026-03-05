namespace Fudie.Security.Http.UnitTests;

public class ICatalogRegistryTests
{
    private static readonly Type Type = typeof(ICatalogRegistry);

    [Fact]
    public void ICatalogRegistry_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareExactlyOneProperty()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(1);
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareExactlySixMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(6);
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareRegisterMethod()
    {
        var method = Type.GetMethod("Register");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(3);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(Endpoint));
        method.GetParameters()[2].ParameterType.Should().Be(typeof(IAggregateDescription));
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareFindEndpointMethod()
    {
        var method = Type.GetMethod("FindEndpoint");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Endpoint));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareFindClassNameMethod()
    {
        var method = Type.GetMethod("FindClassName");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(Endpoint));
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareFindScopeMethod()
    {
        var method = Type.GetMethod("FindScope");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(Endpoint));
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareEndpointMapCountProperty()
    {
        var property = Type.GetProperty("EndpointMapCount");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(int));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareGetAllMethod()
    {
        var method = Type.GetMethod("GetAll");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(IReadOnlyList<CatalogEntry>));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void ICatalogRegistry_ShouldDeclareGetTenantMethod()
    {
        var method = Type.GetMethod("GetTenant");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(IReadOnlyList<CatalogEntry>));
        method.GetParameters().Should().BeEmpty();
    }
}

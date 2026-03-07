namespace Fudie.Security.UnitTests;

public class IFudieContextTests
{
    private static readonly Type Type = typeof(IFudieContext);

    [Fact]
    public void IFudieContext_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareExactlySixProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(6);
    }

    [Fact]
    public void IFudieContext_ShouldDeclareExactlyZeroMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().BeEmpty();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareUserIdProperty()
    {
        var property = Type.GetProperty("UserId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareTenantIdProperty()
    {
        var property = Type.GetProperty("TenantId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareIsOwnerProperty()
    {
        var property = Type.GetProperty("IsOwner");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareIsAuthenticatedProperty()
    {
        var property = Type.GetProperty("IsAuthenticated");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareSessionIdProperty()
    {
        var property = Type.GetProperty("SessionId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieContext_ShouldDeclareAppIdProperty()
    {
        var property = Type.GetProperty("AppId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }
}

namespace Fudie.Security.UnitTests;

public class IFudieUserTests
{
    private static readonly Type Type = typeof(IFudieUser);

    [Fact]
    public void IFudieUser_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareExactlySixProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(6);
    }

    [Fact]
    public void IFudieUser_ShouldDeclareExactlyZeroMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().BeEmpty();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareUserIdProperty()
    {
        var property = Type.GetProperty("UserId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareTenantIdProperty()
    {
        var property = Type.GetProperty("TenantId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareIsOwnerProperty()
    {
        var property = Type.GetProperty("IsOwner");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareIsAuthenticatedProperty()
    {
        var property = Type.GetProperty("IsAuthenticated");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(bool));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareSessionIdProperty()
    {
        var property = Type.GetProperty("SessionId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IFudieUser_ShouldDeclareAppIdProperty()
    {
        var property = Type.GetProperty("AppId");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Guid?));
        property.CanRead.Should().BeTrue();
    }
}

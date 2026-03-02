namespace Fudie.Security.UnitTests;

public class IJwtKeyProviderTests
{
    private static readonly Type Type = typeof(IJwtKeyProvider);

    [Fact]
    public void IJwtKeyProvider_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IJwtKeyProvider_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void IJwtKeyProvider_ShouldDeclareExactlyTwoMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(2);
    }

    [Fact]
    public void IJwtKeyProvider_ShouldDeclareGetPrivateKey()
    {
        var method = Type.GetMethod("GetPrivateKey");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(ECDsa));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void IJwtKeyProvider_ShouldDeclareGetJsonWebKey()
    {
        var method = Type.GetMethod("GetJsonWebKey");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(JsonWebKey));
        method.GetParameters().Should().BeEmpty();
    }
}

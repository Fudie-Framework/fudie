namespace Fudie.Security.Extensions.UnitTests;

public class IJwksApiTests
{
    private static readonly Type Type = typeof(IJwksApi);

    [Fact]
    public void IJwksApi_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IJwksApi_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void IJwksApi_ShouldDeclareExactlyOneMethod()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IJwksApi_ShouldDeclareGetJwksAsyncMethod()
    {
        var method = Type.GetMethod("GetJwksAsync");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<JwksResponse>));
        method.GetParameters().Should().BeEmpty();
    }
}

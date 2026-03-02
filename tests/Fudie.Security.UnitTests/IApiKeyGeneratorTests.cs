namespace Fudie.Security.UnitTests;

public class IApiKeyGeneratorTests
{
    private static readonly Type Type = typeof(IApiKeyGenerator);

    [Fact]
    public void IApiKeyGenerator_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IApiKeyGenerator_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void IApiKeyGenerator_ShouldDeclareExactlyTwoMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(2);
    }

    [Fact]
    public void IApiKeyGenerator_ShouldDeclareGenerateMethod()
    {
        var method = Type.GetMethod("Generate");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(ApiKeyResult));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void IApiKeyGenerator_ShouldDeclareVerifyMethod()
    {
        var method = Type.GetMethod("Verify");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(bool));
        method.GetParameters().Should().HaveCount(3);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[2].ParameterType.Should().Be(typeof(string));
    }
}

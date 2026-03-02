namespace Fudie.Security.UnitTests;

public class ITokenGeneratorTests
{
    private static readonly Type Type = typeof(ITokenGenerator);

    [Fact]
    public void ITokenGenerator_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void ITokenGenerator_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void ITokenGenerator_ShouldDeclareExactlyOneMethod()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void ITokenGenerator_ShouldDeclareGenerateSessionToken()
    {
        var method = Type.GetMethod("GenerateSessionToken");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        var parameters = method.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be(typeof(FudieTokenContext));
    }
}

namespace Fudie.Security.UnitTests;

public class IPasswordHasherTests
{
    private static readonly Type Type = typeof(IPasswordHasher);

    [Fact]
    public void IPasswordHasher_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IPasswordHasher_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void IPasswordHasher_ShouldDeclareExactlyThreeMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(3);
    }

    [Fact]
    public void IPasswordHasher_ShouldDeclareGenerateSaltMethod()
    {
        var method = Type.GetMethod("GenerateSalt");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().BeEmpty();
    }

    [Fact]
    public void IPasswordHasher_ShouldDeclareHashMethod()
    {
        var method = Type.GetMethod("Hash");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(string));
        method.GetParameters().Should().HaveCount(2);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(string));
        method.GetParameters()[1].ParameterType.Should().Be(typeof(string));
    }

    [Fact]
    public void IPasswordHasher_ShouldDeclareVerifyMethod()
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

using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace Fudie.Api.UnitTests;

public class IFeatureModuleTests
{
    private static readonly Type Type = typeof(IFeatureModule);

    [Fact]
    public void IFeatureModule_ShouldBeAnInterface()
    {
        Type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IFeatureModule_ShouldDeclareExactlyZeroProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().BeEmpty();
    }

    [Fact]
    public void IFeatureModule_ShouldDeclareExactlyOneMethod()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().HaveCount(1);
    }

    [Fact]
    public void IFeatureModule_ShouldDeclareAddRoutesMethod()
    {
        var method = Type.GetMethod("AddRoutes");

        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(void));
        method.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].Name.Should().Be("app");
        method.GetParameters()[0].ParameterType.Should().Be(typeof(IEndpointRouteBuilder));
    }
}

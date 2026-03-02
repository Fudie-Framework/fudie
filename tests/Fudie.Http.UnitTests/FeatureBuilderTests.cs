using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fudie.Http.UnitTests;

public class FeatureBuilderTests
{
    private static readonly Type Type = typeof(FeatureBuilder);

    [Fact]
    public void FeatureBuilder_ShouldBeSealedClass()
    {
        Type.IsClass.Should().BeTrue();
        Type.IsSealed.Should().BeTrue();
    }

    [Fact]
    public void FeatureBuilder_ShouldDeclareExactlyThreePublicProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(3);
    }

    [Fact]
    public void FeatureBuilder_ShouldDeclareAppProperty()
    {
        var property = Type.GetProperty("App");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(IApplicationBuilder));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void FeatureBuilder_ShouldDeclareRoutesProperty()
    {
        var property = Type.GetProperty("Routes");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(IEndpointRouteBuilder));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void FeatureBuilder_ShouldDeclareEndpointMappingsProperty()
    {
        var property = Type.GetProperty("EndpointMappings");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(IReadOnlyList<FeatureEndpointMapping>));
        property.CanRead.Should().BeTrue();
    }

    [Fact]
    public void FeatureBuilder_ShouldHaveInternalConstructor()
    {
        var constructors = Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        constructors.Should().BeEmpty();

        var internalConstructor = Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
        internalConstructor.Should().HaveCount(1);
    }

    [Fact]
    public void FeatureBuilder_ShouldDeclareExactlyZeroPublicMethods()
    {
        var methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName).ToArray();
        methods.Should().BeEmpty();
    }
}

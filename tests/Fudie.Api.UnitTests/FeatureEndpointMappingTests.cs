using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Fudie.Api.UnitTests;

public class FeatureEndpointMappingTests
{
    private static readonly Type Type = typeof(FeatureEndpointMapping);

    [Fact]
    public void FeatureEndpointMapping_ShouldBeARecord()
    {
        Type.IsClass.Should().BeTrue();
        Type.GetMethod("<Clone>$").Should().NotBeNull();
    }

    [Fact]
    public void FeatureEndpointMapping_ShouldDeclareExactlyThreeProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(3);
    }

    [Fact]
    public void FeatureEndpointMapping_ShouldDeclareClassNameProperty()
    {
        var property = Type.GetProperty("ClassName");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void FeatureEndpointMapping_ShouldDeclareFeatureNamespaceProperty()
    {
        var property = Type.GetProperty("FeatureNamespace");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void FeatureEndpointMapping_ShouldDeclareEndpointProperty()
    {
        var property = Type.GetProperty("Endpoint");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(Endpoint));
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(), "test");

        var mapping = new FeatureEndpointMapping("MyFeature", "MyApp.Features", endpoint);

        mapping.ClassName.Should().Be("MyFeature");
        mapping.FeatureNamespace.Should().Be("MyApp.Features");
        mapping.Endpoint.Should().BeSameAs(endpoint);
    }

    [Fact]
    public void Constructor_ShouldAllowNullFeatureNamespace()
    {
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(), "test");

        var mapping = new FeatureEndpointMapping("MyFeature", null, endpoint);

        mapping.FeatureNamespace.Should().BeNull();
    }
}

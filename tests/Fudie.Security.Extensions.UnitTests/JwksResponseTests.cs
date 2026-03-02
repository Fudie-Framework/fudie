namespace Fudie.Security.Extensions.UnitTests;

public class JwksResponseTests
{
    private static readonly Type Type = typeof(JwksResponse);

    [Fact]
    public void JwksResponse_ShouldBeARecord()
    {
        Type.GetMethod("<Clone>$").Should().NotBeNull();
    }

    [Fact]
    public void JwksResponse_ShouldDeclareExactlyOneProperty()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(1);
    }

    [Fact]
    public void JwksResponse_ShouldDeclareKeysProperty()
    {
        var property = Type.GetProperty("Keys");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(JwkEntry[]));
    }

    [Fact]
    public void JwksResponse_CanBeConstructedWithKeys()
    {
        var keys = new[] { new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256") };
        var response = new JwksResponse(keys);

        response.Keys.Should().HaveCount(1);
        response.Keys[0].Kid.Should().Be("kid1");
    }

    [Fact]
    public void JwksResponse_EqualityByValue()
    {
        var keys = new[] { new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256") };
        var a = new JwksResponse(keys);
        var b = new JwksResponse(keys);

        a.Should().Be(b);
    }
}

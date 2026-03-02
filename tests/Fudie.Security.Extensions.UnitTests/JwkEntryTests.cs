namespace Fudie.Security.Extensions.UnitTests;

public class JwkEntryTests
{
    private static readonly Type Type = typeof(JwkEntry);

    [Fact]
    public void JwkEntry_ShouldBeARecord()
    {
        Type.GetMethod("<Clone>$").Should().NotBeNull();
    }

    [Fact]
    public void JwkEntry_ShouldDeclareExactlySevenProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(7);
    }

    [Theory]
    [InlineData("Kty", typeof(string))]
    [InlineData("Crv", typeof(string))]
    [InlineData("X", typeof(string))]
    [InlineData("Y", typeof(string))]
    [InlineData("Kid", typeof(string))]
    [InlineData("Use", typeof(string))]
    [InlineData("Alg", typeof(string))]
    public void JwkEntry_ShouldDeclareProperty(string name, Type expectedType)
    {
        var property = Type.GetProperty(name);

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(expectedType);
    }

    [Fact]
    public void JwkEntry_CanBeConstructedWithAllParameters()
    {
        var entry = new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256");

        entry.Kty.Should().Be("EC");
        entry.Crv.Should().Be("P-256");
        entry.X.Should().Be("x");
        entry.Y.Should().Be("y");
        entry.Kid.Should().Be("kid1");
        entry.Use.Should().Be("sig");
        entry.Alg.Should().Be("ES256");
    }

    [Fact]
    public void JwkEntry_EqualityByValue()
    {
        var a = new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256");
        var b = new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256");

        a.Should().Be(b);
    }

    [Fact]
    public void JwkEntry_InequalityByValue()
    {
        var a = new JwkEntry("EC", "P-256", "x", "y", "kid1", "sig", "ES256");
        var b = new JwkEntry("EC", "P-384", "x", "y", "kid2", "sig", "ES384");

        a.Should().NotBe(b);
    }
}

namespace Fudie.Security.UnitTests;

public class ApiKeyResultTests
{
    private static readonly Type Type = typeof(ApiKeyResult);

    [Fact]
    public void ApiKeyResult_ShouldBeARecord()
    {
        Type.GetMethod("<Clone>$").Should().NotBeNull();
    }

    [Fact]
    public void ApiKeyResult_ShouldDeclareExactlyFourProperties()
    {
        Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Should().HaveCount(4);
    }

    [Fact]
    public void ApiKeyResult_ShouldDeclareRawKeyProperty()
    {
        var property = Type.GetProperty("RawKey");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void ApiKeyResult_ShouldDeclareHashProperty()
    {
        var property = Type.GetProperty("Hash");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void ApiKeyResult_ShouldDeclareSaltProperty()
    {
        var property = Type.GetProperty("Salt");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void ApiKeyResult_ShouldDeclarePrefixProperty()
    {
        var property = Type.GetProperty("Prefix");

        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void ApiKeyResult_CanBeConstructedWithAllParameters()
    {
        var result = new ApiKeyResult("rawKey", "hash", "salt", "prefix");

        result.RawKey.Should().Be("rawKey");
        result.Hash.Should().Be("hash");
        result.Salt.Should().Be("salt");
        result.Prefix.Should().Be("prefix");
    }

    [Fact]
    public void ApiKeyResult_EqualityByValue()
    {
        var a = new ApiKeyResult("key", "hash", "salt", "pref");
        var b = new ApiKeyResult("key", "hash", "salt", "pref");

        a.Should().Be(b);
    }

    [Fact]
    public void ApiKeyResult_InequalityByValue()
    {
        var a = new ApiKeyResult("key1", "hash", "salt", "pref");
        var b = new ApiKeyResult("key2", "hash", "salt", "pref");

        a.Should().NotBe(b);
    }
}

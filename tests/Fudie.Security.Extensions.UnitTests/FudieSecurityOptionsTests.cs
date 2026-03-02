namespace Fudie.Security.Extensions.UnitTests;

public class FudieSecurityOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeFudieSecurity()
    {
        FudieSecurityOptions.SectionName.Should().Be("Fudie:Security");
    }

    [Fact]
    public void JwksUrl_DefaultShouldBeEmpty()
    {
        var options = new FudieSecurityOptions();

        options.JwksUrl.Should().BeEmpty();
    }

    [Fact]
    public void CacheRefreshMinutes_DefaultShouldBe60()
    {
        var options = new FudieSecurityOptions();

        options.CacheRefreshMinutes.Should().Be(60);
    }

    [Fact]
    public void JwksUrl_CanBeSet()
    {
        var options = new FudieSecurityOptions { JwksUrl = "http://localhost/jwks" };

        options.JwksUrl.Should().Be("http://localhost/jwks");
    }

    [Fact]
    public void CacheRefreshMinutes_CanBeSet()
    {
        var options = new FudieSecurityOptions { CacheRefreshMinutes = 30 };

        options.CacheRefreshMinutes.Should().Be(30);
    }
}

namespace Fudie.Security.UnitTests;

public class FudieTokenContextTests
{
    [Fact]
    public void ShouldCreateWithAllProperties()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var groups = new[] { "menu:read", "menu:write" };
        var additionalScopes = new[] { "admin:manage" };
        var excludedScopes = new[] { "billing:delete" };

        var context = new FudieTokenContext(userId, tenantId, false, groups, additionalScopes, excludedScopes);

        context.UserId.Should().Be(userId);
        context.TenantId.Should().Be(tenantId);
        context.IsOwner.Should().BeFalse();
        context.Groups.Should().BeEquivalentTo(groups);
        context.AdditionalScopes.Should().BeEquivalentTo(additionalScopes);
        context.ExcludedScopes.Should().BeEquivalentTo(excludedScopes);
    }

    [Fact]
    public void ShouldAllowNullTenantId()
    {
        var context = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        context.TenantId.Should().BeNull();
    }

    [Fact]
    public void ShouldSupportOwnerFlag()
    {
        var context = new FudieTokenContext(Guid.NewGuid(), Guid.NewGuid(), true, [], [], []);

        context.IsOwner.Should().BeTrue();
    }

    [Fact]
    public void ShouldSupportSessionId()
    {
        var sessionId = Guid.NewGuid();
        var context = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], [], sessionId);

        context.SessionId.Should().Be(sessionId);
        context.AppId.Should().BeNull();
    }

    [Fact]
    public void ShouldSupportAppId()
    {
        var appId = Guid.NewGuid();
        var context = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], [], AppId: appId);

        context.AppId.Should().Be(appId);
        context.SessionId.Should().BeNull();
    }

    [Fact]
    public void ShouldDefaultSessionIdAndAppIdToNull()
    {
        var context = new FudieTokenContext(Guid.NewGuid(), null, false, [], [], []);

        context.SessionId.Should().BeNull();
        context.AppId.Should().BeNull();
    }

}

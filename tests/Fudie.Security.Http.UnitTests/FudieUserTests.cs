namespace Fudie.Security.Http.UnitTests;

public class FudieUserTests
{
    #region No HttpContext

    [Fact]
    public void IsAuthenticated_WithNoHttpContext_ShouldReturnFalse()
    {
        var user = CreateFudieUser(httpContext: null);
        user.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void UserId_WithNoHttpContext_ShouldReturnNull()
    {
        var user = CreateFudieUser(httpContext: null);
        user.UserId.Should().BeNull();
    }

    [Fact]
    public void TenantId_WithNoHttpContext_ShouldReturnNull()
    {
        var user = CreateFudieUser(httpContext: null);
        user.TenantId.Should().BeNull();
    }

    [Fact]
    public void IsOwner_WithNoHttpContext_ShouldReturnFalse()
    {
        var user = CreateFudieUser(httpContext: null);
        user.IsOwner.Should().BeFalse();
    }

    #endregion

    #region Unauthenticated User

    [Fact]
    public void IsAuthenticated_WithAnonymousUser_ShouldReturnFalse()
    {
        var context = new DefaultHttpContext();
        var user = CreateFudieUser(context);
        user.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void UserId_WithAnonymousUser_ShouldReturnNull()
    {
        var context = new DefaultHttpContext();
        var user = CreateFudieUser(context);
        user.UserId.Should().BeNull();
    }

    #endregion

    #region Authenticated User

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ShouldReturnTrue()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var user = CreateFudieUser(context);
        user.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void UserId_WithValidSubClaim_ShouldReturnGuid()
    {
        var userId = Guid.NewGuid();
        var context = CreateAuthenticatedContext(userId);
        var user = CreateFudieUser(context);
        user.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WithInvalidSubClaim_ShouldReturnNull()
    {
        var context = CreateContextWithClaims(new Claim("sub", "not-a-guid"));
        var user = CreateFudieUser(context);
        user.UserId.Should().BeNull();
    }

    [Fact]
    public void TenantId_WithValidTidClaim_ShouldReturnGuid()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("tid", tenantId.ToString()));
        var user = CreateFudieUser(context);
        user.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void TenantId_WithNoTidClaim_ShouldReturnNull()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var user = CreateFudieUser(context);
        user.TenantId.Should().BeNull();
    }

    [Fact]
    public void TenantId_WithInvalidTidClaim_ShouldReturnNull()
    {
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("tid", "not-a-guid"));
        var user = CreateFudieUser(context);
        user.TenantId.Should().BeNull();
    }

    [Fact]
    public void IsOwner_WithOwnerClaim_ShouldReturnTrue()
    {
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("owner", "true"));
        var user = CreateFudieUser(context);
        user.IsOwner.Should().BeTrue();
    }

    [Fact]
    public void IsOwner_WithoutOwnerClaim_ShouldReturnFalse()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var user = CreateFudieUser(context);
        user.IsOwner.Should().BeFalse();
    }

    [Fact]
    public void IsOwner_WithOwnerFalse_ShouldReturnFalse()
    {
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("owner", "false"));
        var user = CreateFudieUser(context);
        user.IsOwner.Should().BeFalse();
    }

    [Fact]
    public void SessionId_WithValidSidClaim_ShouldReturnGuid()
    {
        var sessionId = Guid.NewGuid();
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("sid", sessionId.ToString()));
        var user = CreateFudieUser(context);
        user.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void SessionId_WithNoSidClaim_ShouldReturnNull()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var user = CreateFudieUser(context);
        user.SessionId.Should().BeNull();
    }

    [Fact]
    public void SessionId_WithInvalidSidClaim_ShouldReturnNull()
    {
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("sid", "not-a-guid"));
        var user = CreateFudieUser(context);
        user.SessionId.Should().BeNull();
    }

    [Fact]
    public void AppId_WithValidAppClaim_ShouldReturnGuid()
    {
        var appId = Guid.NewGuid();
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("app", appId.ToString()));
        var user = CreateFudieUser(context);
        user.AppId.Should().Be(appId);
    }

    [Fact]
    public void AppId_WithNoAppClaim_ShouldReturnNull()
    {
        var context = CreateAuthenticatedContext(Guid.NewGuid());
        var user = CreateFudieUser(context);
        user.AppId.Should().BeNull();
    }

    [Fact]
    public void AppId_WithInvalidAppClaim_ShouldReturnNull()
    {
        var context = CreateContextWithClaims(
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("app", "not-a-guid"));
        var user = CreateFudieUser(context);
        user.AppId.Should().BeNull();
    }

    [Fact]
    public void SessionId_WithNoHttpContext_ShouldReturnNull()
    {
        var user = CreateFudieUser(httpContext: null);
        user.SessionId.Should().BeNull();
    }

    [Fact]
    public void AppId_WithNoHttpContext_ShouldReturnNull()
    {
        var user = CreateFudieUser(httpContext: null);
        user.AppId.Should().BeNull();
    }

    #endregion

    #region Identity Edge Cases

    [Fact]
    public void IsAuthenticated_WithPrincipalButNoIdentity_ShouldReturnFalse()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal();
        var user = CreateFudieUser(context);
        user.IsAuthenticated.Should().BeFalse();
    }

    #endregion

    #region IFudieUser Contract

    [Fact]
    public void FudieUser_ShouldImplementIFudieUser()
    {
        typeof(FudieUser).Should().BeAssignableTo<IFudieUser>();
    }

    #endregion

    #region Helpers

    private static FudieUser CreateFudieUser(HttpContext? httpContext)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);
        return new FudieUser(accessor.Object);
    }

    private static HttpContext CreateAuthenticatedContext(Guid userId)
    {
        return CreateContextWithClaims(new Claim("sub", userId.ToString()));
    }

    private static HttpContext CreateContextWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "FudieToken");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        return context;
    }

    #endregion
}

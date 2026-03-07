namespace Fudie.Security.Http.UnitTests;

public class FudieContextExtensionsTests
{
    private readonly Mock<IFudieContext> _context = new();

    [Fact]
    public void RequiredUserId_WithValue_ShouldReturnGuid()
    {
        var id = Guid.NewGuid();
        _context.Setup(c => c.UserId).Returns(id);

        _context.Object.RequiredUserId().Should().Be(id);
    }

    [Fact]
    public void RequiredUserId_WithNull_ShouldThrowUnauthorizedException()
    {
        _context.Setup(c => c.UserId).Returns((Guid?)null);

        var act = () => _context.Object.RequiredUserId();

        act.Should().Throw<UnauthorizedException>();
    }

    [Fact]
    public void RequiredTenantId_WithValue_ShouldReturnGuid()
    {
        var id = Guid.NewGuid();
        _context.Setup(c => c.TenantId).Returns(id);

        _context.Object.RequiredTenantId().Should().Be(id);
    }

    [Fact]
    public void RequiredTenantId_WithNull_ShouldThrowUnauthorizedException()
    {
        _context.Setup(c => c.TenantId).Returns((Guid?)null);

        var act = () => _context.Object.RequiredTenantId();

        act.Should().Throw<UnauthorizedException>();
    }

    [Fact]
    public void RequiredSessionId_WithValue_ShouldReturnGuid()
    {
        var id = Guid.NewGuid();
        _context.Setup(c => c.SessionId).Returns(id);

        _context.Object.RequiredSessionId().Should().Be(id);
    }

    [Fact]
    public void RequiredSessionId_WithNull_ShouldThrowUnauthorizedException()
    {
        _context.Setup(c => c.SessionId).Returns((Guid?)null);

        var act = () => _context.Object.RequiredSessionId();

        act.Should().Throw<UnauthorizedException>();
    }

    [Fact]
    public void RequiredAppId_WithValue_ShouldReturnGuid()
    {
        var id = Guid.NewGuid();
        _context.Setup(c => c.AppId).Returns(id);

        _context.Object.RequiredAppId().Should().Be(id);
    }

    [Fact]
    public void RequiredAppId_WithNull_ShouldThrowUnauthorizedException()
    {
        _context.Setup(c => c.AppId).Returns((Guid?)null);

        var act = () => _context.Object.RequiredAppId();

        act.Should().Throw<UnauthorizedException>();
    }
}

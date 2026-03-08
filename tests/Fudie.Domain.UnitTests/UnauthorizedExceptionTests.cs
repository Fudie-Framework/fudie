
namespace Fudie.Domain.UnitTests;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Session expired";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetErrorCodeToNull()
    {
        var exception = new UnauthorizedException("test");

        exception.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_ShouldSetMessage()
    {
        var exception = new UnauthorizedException("Not allowed", "Order.Owner.NotOwner");

        exception.Message.Should().Be("Not allowed");
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_ShouldSetErrorCode()
    {
        var exception = new UnauthorizedException("Not allowed", "Order.Owner.NotOwner");

        exception.ErrorCode.Should().Be("Order.Owner.NotOwner");
    }

    [Fact]
    public void UnauthorizedException_ShouldInheritFromException()
    {
        var exception = new UnauthorizedException("test");

        exception.Should().BeAssignableTo<Exception>();
    }
}

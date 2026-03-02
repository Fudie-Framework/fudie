namespace Fudie.Domain.UnitTests;

public class ConflictExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var message = "Resource already exists";

        var exception = new ConflictException(message);

        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ConflictException_ShouldInheritFromException()
    {
        var exception = new ConflictException("test");

        exception.Should().BeAssignableTo<Exception>();
    }
}

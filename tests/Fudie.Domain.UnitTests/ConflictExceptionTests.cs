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
    public void Constructor_WithMessage_ShouldSetErrorCodeToNull()
    {
        var exception = new ConflictException("test");

        exception.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_ShouldSetMessage()
    {
        var exception = new ConflictException("Already exists", "Customer.Slug.AlreadyExists");

        exception.Message.Should().Be("Already exists");
    }

    [Fact]
    public void Constructor_WithMessageAndErrorCode_ShouldSetErrorCode()
    {
        var exception = new ConflictException("Already exists", "Customer.Slug.AlreadyExists");

        exception.ErrorCode.Should().Be("Customer.Slug.AlreadyExists");
    }

    [Fact]
    public void ConflictException_ShouldInheritFromException()
    {
        var exception = new ConflictException("test");

        exception.Should().BeAssignableTo<Exception>();
    }
}

namespace Fudie.Validation.UnitTests;

public class ValidationGuardTests
{
    [Fact]
    public void ThrowIf_WhenConditionIsTrue_ShouldThrowValidationException()
    {
        var act = () => ValidationGuard.ThrowIf(true, "Invalid value", "Field");

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle();
        exception.Errors.First().ErrorMessage.Should().Be("Invalid value");
        exception.Errors.First().PropertyName.Should().Be("Field");
    }

    [Fact]
    public void ThrowIf_WhenConditionIsFalse_ShouldNotThrow()
    {
        var act = () => ValidationGuard.ThrowIf(false, "Invalid value", "Field");

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIf_WithDefaultPropertyName_ShouldUseEmptyString()
    {
        var act = () => ValidationGuard.ThrowIf(true, "Invalid value");

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().PropertyName.Should().BeEmpty();
    }

    [Fact]
    public void ThrowIfNot_WhenConditionIsFalse_ShouldThrowValidationException()
    {
        var act = () => ValidationGuard.ThrowIfNot(false, "Required", "Name");

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle();
        exception.Errors.First().ErrorMessage.Should().Be("Required");
        exception.Errors.First().PropertyName.Should().Be("Name");
    }

    [Fact]
    public void ThrowIfNot_WhenConditionIsTrue_ShouldNotThrow()
    {
        var act = () => ValidationGuard.ThrowIfNot(true, "Required", "Name");

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNot_WithDefaultPropertyName_ShouldUseEmptyString()
    {
        var act = () => ValidationGuard.ThrowIfNot(false, "Required");

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().PropertyName.Should().BeEmpty();
    }

    #region ErrorCode Overloads

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldThrowWithPropertyAndMessage()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "Name is required");

        var act = () => ValidationGuard.ThrowIf(true, errorCode);

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("Name");
        exception.Errors.First().ErrorMessage.Should().Be("Name is required");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldSetErrorCodeOnFailure()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "Name is required");

        var act = () => ValidationGuard.ThrowIf(true, errorCode);

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().ErrorCode.Should().Be("Customer.Name.Required");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsFalse_ShouldNotThrow()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "Name is required");

        var act = () => ValidationGuard.ThrowIf(false, errorCode);

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNot_WithErrorCode_WhenConditionIsFalse_ShouldThrowWithPropertyAndMessage()
    {
        var errorCode = new ErrorCode("Customer", "Email", "InvalidFormat", "Email is invalid");

        var act = () => ValidationGuard.ThrowIfNot(false, errorCode);

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("Email");
        exception.Errors.First().ErrorMessage.Should().Be("Email is invalid");
    }

    [Fact]
    public void ThrowIfNot_WithErrorCode_WhenConditionIsFalse_ShouldSetErrorCodeOnFailure()
    {
        var errorCode = new ErrorCode("Customer", "Email", "InvalidFormat", "Email is invalid");

        var act = () => ValidationGuard.ThrowIfNot(false, errorCode);

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.First().ErrorCode.Should().Be("Customer.Email.InvalidFormat");
    }

    [Fact]
    public void ThrowIfNot_WithErrorCode_WhenConditionIsTrue_ShouldNotThrow()
    {
        var errorCode = new ErrorCode("Customer", "Email", "InvalidFormat", "Email is invalid");

        var act = () => ValidationGuard.ThrowIfNot(true, errorCode);

        act.Should().NotThrow();
    }

    #endregion
}

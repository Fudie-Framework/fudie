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
}

namespace Fudie.Validation.UnitTests;

public class UnauthorizedGuardTests
{
    [Fact]
    public void ThrowIf_WhenConditionIsTrue_ShouldThrowUnauthorizedException()
    {
        // Arrange & Act
        var act = () => UnauthorizedGuard.ThrowIf(true, "Session expired");

        // Assert
        act.Should().Throw<UnauthorizedException>().WithMessage("Session expired");
    }

    [Fact]
    public void ThrowIf_WhenConditionIsFalse_ShouldNotThrow()
    {
        // Arrange & Act
        var act = () => UnauthorizedGuard.ThrowIf(false, "Session expired");

        // Assert
        act.Should().NotThrow();
    }

    #region ErrorCode Overload

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldThrowWithMessage()
    {
        var errorCode = new ErrorCode("Order", "Owner", "NotOwner", "Not the owner", ErrorCodeCategory.Guard);

        var act = () => UnauthorizedGuard.ThrowIf(true, errorCode);

        act.Should().Throw<UnauthorizedException>()
            .WithMessage("Not the owner");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldSetErrorCodeOnException()
    {
        var errorCode = new ErrorCode("Order", "Owner", "NotOwner", "Not the owner", ErrorCodeCategory.Guard);

        var act = () => UnauthorizedGuard.ThrowIf(true, errorCode);

        act.Should().Throw<UnauthorizedException>()
            .Which.ErrorCode.Should().Be("Order.Owner.NotOwner");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsFalse_ShouldNotThrow()
    {
        var errorCode = new ErrorCode("Order", "Owner", "NotOwner", "Not the owner", ErrorCodeCategory.Guard);

        var act = () => UnauthorizedGuard.ThrowIf(false, errorCode);

        act.Should().NotThrow();
    }

    #endregion
}

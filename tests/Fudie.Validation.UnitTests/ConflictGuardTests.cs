namespace Fudie.Validation.UnitTests;

public class ConflictGuardTests
{
    [Fact]
    public void ThrowIf_WhenConditionIsTrue_ShouldThrowConflictException()
    {
        var act = () => ConflictGuard.ThrowIf(true, "Already exists");

        act.Should().Throw<ConflictException>()
            .WithMessage("Already exists");
    }

    [Fact]
    public void ThrowIf_WhenConditionIsFalse_ShouldNotThrow()
    {
        var act = () => ConflictGuard.ThrowIf(false, "Already exists");

        act.Should().NotThrow();
    }

    #region ErrorCode Overload

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldThrowWithMessage()
    {
        var errorCode = new ErrorCode("Customer", "Slug", "AlreadyExists", "Slug already taken", ErrorCodeCategory.Guard);

        var act = () => ConflictGuard.ThrowIf(true, errorCode);

        act.Should().Throw<ConflictException>()
            .WithMessage("Slug already taken");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsTrue_ShouldSetErrorCodeOnException()
    {
        var errorCode = new ErrorCode("Customer", "Slug", "AlreadyExists", "Slug already taken", ErrorCodeCategory.Guard);

        var act = () => ConflictGuard.ThrowIf(true, errorCode);

        act.Should().Throw<ConflictException>()
            .Which.ErrorCode.Should().Be("Customer.Slug.AlreadyExists");
    }

    [Fact]
    public void ThrowIf_WithErrorCode_WhenConditionIsFalse_ShouldNotThrow()
    {
        var errorCode = new ErrorCode("Customer", "Slug", "AlreadyExists", "Slug already taken", ErrorCodeCategory.Guard);

        var act = () => ConflictGuard.ThrowIf(false, errorCode);

        act.Should().NotThrow();
    }

    #endregion
}

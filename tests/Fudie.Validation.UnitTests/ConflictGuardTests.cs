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
}

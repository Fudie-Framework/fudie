namespace Fudie.Validation.UnitTests;

public class ErrorCodeCategoryTests
{
    [Fact]
    public void FluentValidation_ShouldHaveValue1()
    {
        ((int)ErrorCodeCategory.FluentValidation).Should().Be(1);
    }

    [Fact]
    public void Guard_ShouldHaveValue2()
    {
        ((int)ErrorCodeCategory.Guard).Should().Be(2);
    }

    [Fact]
    public void Enum_ShouldHaveExactlyTwoMembers()
    {
        Enum.GetValues<ErrorCodeCategory>().Should().HaveCount(2);
    }
}

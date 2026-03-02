namespace Fudie.Validation.UnitTests;

public class ValidatorExtensionsTests
{
    private class TestEntity(Guid id) : Entity<Guid>(id)
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestEntityValidator : AbstractValidator<TestEntity>
    {
        public TestEntityValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    private record TestDto(string Value);

    private class TestDtoValidator : AbstractValidator<TestDto>
    {
        public TestDtoValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    [Fact]
    public void ValidateOrThrow_WithValidEntity_ShouldReturnEntity()
    {
        var entity = new TestEntity(Guid.NewGuid()) { Name = "Valid" };
        var validator = new TestEntityValidator();

        var result = validator.ValidateOrThrow(entity);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public void ValidateOrThrow_WithEmptyGuidId_ShouldThrowValidationException()
    {
        var entity = new TestEntity(Guid.Empty) { Name = "Valid" };
        var validator = new TestEntityValidator();

        var act = () => validator.ValidateOrThrow(entity);

        var exception = act.Should().Throw<ValidationException>().Which;
        exception.Errors.Should().ContainSingle();
        exception.Errors.First().PropertyName.Should().Be("Id");
        exception.Errors.First().ErrorMessage.Should().Be("Id cannot be empty");
    }

    [Fact]
    public void ValidateOrThrow_WithInvalidEntity_ShouldThrowValidationException()
    {
        var entity = new TestEntity(Guid.NewGuid()) { Name = "" };
        var validator = new TestEntityValidator();

        var act = () => validator.ValidateOrThrow(entity);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValidateOrThrow_WithNonEntityType_ShouldSkipIdCheck()
    {
        var dto = new TestDto("Valid");
        var validator = new TestDtoValidator();

        var result = validator.ValidateOrThrow(dto);

        result.Should().BeSameAs(dto);
    }

    [Fact]
    public void ValidateOrThrow_WithInvalidNonEntityType_ShouldThrowValidationException()
    {
        var dto = new TestDto("");
        var validator = new TestDtoValidator();

        var act = () => validator.ValidateOrThrow(dto);

        act.Should().Throw<ValidationException>();
    }
}

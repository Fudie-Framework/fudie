namespace Fudie.Validation.UnitTests;

public class NotFoundGuardTests
{
    private class TestEntity;

    [Fact]
    public void ThrowIfNull_WhenEntityIsNull_ShouldThrowKeyNotFoundException()
    {
        TestEntity? entity = null;

        var act = () => NotFoundGuard.ThrowIfNull(entity);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("TestEntity not found");
    }

    [Fact]
    public void ThrowIfNull_WhenEntityIsNotNull_ShouldReturnEntity()
    {
        var entity = new TestEntity();

        var result = NotFoundGuard.ThrowIfNull(entity);

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public void ThrowIfNull_WithId_WhenEntityIsNull_ShouldThrowWithIdInMessage()
    {
        TestEntity? entity = null;
        var id = Guid.NewGuid();

        var act = () => NotFoundGuard.ThrowIfNull(entity, id);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"TestEntity with Id '{id}' not found");
    }

    [Fact]
    public void ThrowIfNull_WithId_WhenEntityIsNotNull_ShouldReturnEntity()
    {
        var entity = new TestEntity();

        var result = NotFoundGuard.ThrowIfNull(entity, Guid.NewGuid());

        result.Should().BeSameAs(entity);
    }

    [Fact]
    public void ThrowIfNull_WithMessage_WhenEntityIsNull_ShouldThrowWithCustomMessage()
    {
        TestEntity? entity = null;

        var act = () => NotFoundGuard.ThrowIfNull(entity, "Custom not found message");

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Custom not found message");
    }

    [Fact]
    public void ThrowIfNull_WithMessage_WhenEntityIsNotNull_ShouldReturnEntity()
    {
        var entity = new TestEntity();

        var result = NotFoundGuard.ThrowIfNull(entity, "Custom not found message");

        result.Should().BeSameAs(entity);
    }
}

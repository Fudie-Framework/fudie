namespace Fudie.Security.Jwt.UnitTests;

public class ISigningKeyProviderTests
{
    [Fact]
    public void Interface_should_have_exactly_one_method()
    {
        var methods = typeof(ISigningKeyProvider)
            .GetMethods()
            .Where(m => !m.IsSpecialName)
            .ToArray();

        methods.Should().HaveCount(1);
    }

    [Fact]
    public void GetSigningKeysAsync_should_return_task_of_enumerable_security_key()
    {
        var method = typeof(ISigningKeyProvider).GetMethod(nameof(ISigningKeyProvider.GetSigningKeysAsync))!;
        method.ReturnType.Should().Be(typeof(Task<IEnumerable<SecurityKey>>));
        method.GetParameters().Should().BeEmpty();
    }
}

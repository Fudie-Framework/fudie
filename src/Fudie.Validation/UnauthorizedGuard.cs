namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="Domain.UnauthorizedException"/>
/// when an authorization condition is met.
/// </summary>
public static class UnauthorizedGuard
{
    /// <summary>
    /// Throws a <see cref="Domain.UnauthorizedException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The authorization failure message.</param>
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new Domain.UnauthorizedException(message);
    }
}

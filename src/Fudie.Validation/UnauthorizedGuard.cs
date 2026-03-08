namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="UnauthorizedException"/>
/// when an authorization condition is met.
/// </summary>
public static class UnauthorizedGuard
{
    /// <summary>
    /// Throws a <see cref="UnauthorizedException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The authorization failure message.</param>
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new UnauthorizedException(message);
    }

    /// <summary>
    /// Throws a <see cref="UnauthorizedException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorCode">The error code with message and metadata.</param>
    public static void ThrowIf(bool condition, ErrorCode errorCode)
    {
        if (condition)
            throw new UnauthorizedException(errorCode.Message, errorCode.Code);
    }
}

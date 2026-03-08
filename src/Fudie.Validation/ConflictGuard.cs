namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="ConflictException"/>
/// when a conflict condition is met.
/// </summary>
public static class ConflictGuard
{
    /// <summary>
    /// Throws a <see cref="ConflictException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The conflict message.</param>
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new ConflictException(message);
    }

    /// <summary>
    /// Throws a <see cref="ConflictException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorCode">The error code with message and metadata.</param>
    public static void ThrowIf(bool condition, ErrorCode errorCode)
    {
        if (condition)
            throw new ConflictException(errorCode.Message, errorCode.Code);
    }
}


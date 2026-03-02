namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="Domain.ConflictException"/>
/// when a conflict condition is met.
/// </summary>
public static class ConflictGuard
{
    /// <summary>
    /// Throws a <see cref="Domain.ConflictException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The conflict message.</param>
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
            throw new Domain.ConflictException(message);
    }
}


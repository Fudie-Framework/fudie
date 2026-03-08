namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="ValidationException"/>
/// when a validation condition is met or not met.
/// </summary>
public static class ValidationGuard
{
    /// <summary>
    /// Throws a <see cref="ValidationException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    public static void ThrowIf(bool condition, string message, string propertyName = "")
    {
        if (condition)
            throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }

    /// <summary>
    /// Throws a <see cref="ValidationException"/> when <paramref name="condition"/> is <c>false</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    public static void ThrowIfNot(bool condition, string message, string propertyName = "")
    {
        if (!condition)
            throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }

    /// <summary>
    /// Throws a <see cref="ValidationException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorCode">The error code with message and metadata.</param>
    public static void ThrowIf(bool condition, ErrorCode errorCode)
    {
        if (condition)
            throw new ValidationException([
                new ValidationFailure(errorCode.Property, errorCode.Message)
                {
                    ErrorCode = errorCode.Code
                }
            ]);
    }

    /// <summary>
    /// Throws a <see cref="ValidationException"/> when <paramref name="condition"/> is <c>false</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorCode">The error code with message and metadata.</param>
    public static void ThrowIfNot(bool condition, ErrorCode errorCode)
    {
        if (!condition)
            throw new ValidationException([
                new ValidationFailure(errorCode.Property, errorCode.Message)
                {
                    ErrorCode = errorCode.Code
                }
            ]);
    }
}
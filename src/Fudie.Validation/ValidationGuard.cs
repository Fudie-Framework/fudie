namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="FluentValidation.ValidationException"/>
/// when a validation condition is met or not met.
/// </summary>
public static class ValidationGuard
{
    /// <summary>
    /// Throws a <see cref="FluentValidation.ValidationException"/> when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    public static void ThrowIf(bool condition, string message, string propertyName = "")
    {
        if (condition)
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(propertyName, message)]);
    }

    /// <summary>
    /// Throws a <see cref="FluentValidation.ValidationException"/> when <paramref name="condition"/> is <c>false</c>.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    public static void ThrowIfNot(bool condition, string message, string propertyName = "")
    {
        if (!condition)
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(propertyName, message)]);
    }
}
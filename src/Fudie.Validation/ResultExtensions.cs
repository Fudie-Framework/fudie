namespace Fudie.Validation;

/// <summary>
/// Extension methods for <see cref="Result"/> and <see cref="Result{T}"/>
/// that unwrap a successful value or throw a <see cref="ValidationException"/>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Returns the value of a successful result or throws a
    /// <see cref="ValidationException"/> containing the validation errors.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to unwrap.</param>
    /// <returns>The value when the result is successful.</returns>
    public static T ValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            var failures = result.Errors.Select(e =>
                new ValidationFailure(e.PropertyName, e.ErrorMessage));

            throw new ValidationException(failures);
        }

        return result.Value!;
    }

    /// <summary>
    /// Verifies the result is successful or throws a
    /// <see cref="ValidationException"/> containing the validation errors.
    /// </summary>
    /// <param name="result">The result to verify.</param>
    public static void SuccessOrThrow(this Result result)
    {
        if (result.IsFailure)
        {
            var failures = result.Errors.Select(e =>
                new ValidationFailure(e.PropertyName, e.ErrorMessage));

            throw new ValidationException(failures);
        }
    }
}

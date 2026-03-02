namespace Fudie.Validation;

/// <summary>
/// Extension methods for <see cref="IValidator{T}"/>.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Validates the given instance and returns it when valid.
    /// Throws a <see cref="ValidationException"/> when validation fails.
    /// For entities with a <see cref="Guid"/> identifier, also verifies the Id is not empty.
    /// </summary>
    /// <typeparam name="T">The type of the instance to validate.</typeparam>
    /// <param name="validator">The FluentValidation validator.</param>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>The validated instance.</returns>
    public static T ValidateOrThrow<T>(this IValidator<T> validator, T instance)
    {
        if (instance is Entity<Guid> entity && entity.Id == Guid.Empty)
        {
            throw new ValidationException([
                new ValidationFailure("Id", "Id cannot be empty")
            ]);
        }

        var result = validator.Validate(instance);

        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return instance;
    }
}

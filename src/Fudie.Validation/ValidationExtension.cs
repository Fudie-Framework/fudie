namespace Fudie.Validation;

/// <summary>
/// Extension methods for <see cref="FluentValidation.IValidator{T}"/>.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Validates the given instance and returns it when valid.
    /// Throws a <see cref="FluentValidation.ValidationException"/> when validation fails.
    /// For entities with a <see cref="Guid"/> identifier, also verifies the Id is not empty.
    /// </summary>
    /// <typeparam name="T">The type of the instance to validate.</typeparam>
    /// <param name="validator">The FluentValidation validator.</param>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>The validated instance.</returns>
    public static T ValidateOrThrow<T>(this FluentValidation.IValidator<T> validator, T instance)
    {
        if (instance is Domain.Entity<Guid> entity && entity.Id == Guid.Empty)
        {
            throw new FluentValidation.ValidationException([
                new FluentValidation.Results.ValidationFailure("Id", "Id cannot be empty")
            ]);
        }

        var result = validator.Validate(instance);

        if (!result.IsValid)
            throw new FluentValidation.ValidationException(result.Errors);

        return instance;
    }
}

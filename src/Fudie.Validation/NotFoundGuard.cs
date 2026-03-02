namespace Fudie.Validation;

/// <summary>
/// Static guard that throws a <see cref="KeyNotFoundException"/>
/// when an entity is not found.
/// </summary>
public static class NotFoundGuard
{
    /// <summary>
    /// Throws a <see cref="KeyNotFoundException"/> when <paramref name="entity"/> is <c>null</c>.
    /// Message: "<c>{TypeName} not found</c>".
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <returns>The entity when it is not <c>null</c>.</returns>
    public static T ThrowIfNull<T>(T? entity) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(T).Name} not found");

        return entity;
    }

    /// <summary>
    /// Throws a <see cref="KeyNotFoundException"/> when <paramref name="entity"/> is <c>null</c>.
    /// Message: "<c>{TypeName} with Id '{id}' not found</c>".
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <param name="id">The identifier used in the error message.</param>
    /// <returns>The entity when it is not <c>null</c>.</returns>
    public static T ThrowIfNull<T, TId>(T? entity, TId id) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(T).Name} with Id '{id}' not found");

        return entity;
    }

    /// <summary>
    /// Throws a <see cref="KeyNotFoundException"/> when <paramref name="entity"/> is <c>null</c>
    /// using a custom message.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <param name="message">The custom error message.</param>
    /// <returns>The entity when it is not <c>null</c>.</returns>
    public static T ThrowIfNull<T>(T? entity, string message) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException(message);

        return entity;
    }
}
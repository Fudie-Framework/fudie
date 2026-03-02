namespace Fudie.Domain;

/// <summary>
/// Abstract domain command for transforming an existing value object with additional data.
/// </summary>
/// <typeparam name="TCommand">The type of the command carrying transformation data.</typeparam>
/// <typeparam name="TValueObject">The type of the value object to transform.</typeparam>
public abstract class AbstractTransformCommand<TCommand, TValueObject>
    where TCommand : class
    where TValueObject : class
{
    /// <summary>
    /// Executes the value object transformation.
    /// </summary>
    /// <param name="current">The existing value object to transform.</param>
    /// <param name="command">The data used for the transformation.</param>
    /// <returns>The transformed value object.</returns>
    public abstract TValueObject Execute(TValueObject current, TCommand command);
}

/// <summary>
/// Abstract domain command for transforming an existing value object without additional data.
/// </summary>
/// <typeparam name="TValueObject">The type of the value object to transform.</typeparam>
public abstract class AbstractTransformCommand<TValueObject>
    where TValueObject : class
{
    /// <summary>
    /// Executes the value object transformation.
    /// </summary>
    /// <param name="current">The existing value object to transform.</param>
    /// <returns>The transformed value object.</returns>
    public abstract TValueObject Execute(TValueObject current);
}

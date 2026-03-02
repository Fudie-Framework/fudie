namespace Fudie.Domain;

/// <summary>
/// Abstract domain command for creating a new entity.
/// </summary>
/// <typeparam name="TCommand">The type of the command carrying creation data.</typeparam>
/// <typeparam name="TEntity">The type of the entity to create.</typeparam>
public abstract class AbstractCreateCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : class
{
    /// <summary>
    /// Executes the entity creation.
    /// </summary>
    /// <param name="command">The data used to create the entity.</param>
    /// <returns>The newly created entity.</returns>
    public abstract TEntity Execute(TCommand command);
}
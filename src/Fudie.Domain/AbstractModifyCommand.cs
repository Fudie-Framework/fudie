namespace Fudie.Domain;

/// <summary>
/// Abstract domain command for modifying an existing entity without additional data.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to modify.</typeparam>
public abstract class AbstractModifyCommand<TEntity>
    where TEntity : IEntity
{
    /// <summary>
    /// Executes the entity modification.
    /// </summary>
    /// <param name="entity">The existing entity to modify.</param>
    /// <returns>The modified entity.</returns>
    public abstract TEntity Execute(TEntity entity);

    /// <summary>
    /// Executes the modification asynchronously by awaiting the entity task.
    /// </summary>
    /// <param name="entityTask">A task that resolves to the entity to modify.</param>
    /// <returns>The modified entity.</returns>
    public async Task<TEntity> ExecuteAsync(Task<TEntity> entityTask)
    {
        var entity = await entityTask;
        return Execute(entity);
    }
}

/// <summary>
/// Abstract domain command for modifying an existing entity with additional data.
/// </summary>
/// <typeparam name="TCommand">The type of the command carrying modification data.</typeparam>
/// <typeparam name="TEntity">The type of the entity to modify.</typeparam>
public abstract class AbstractModifyCommand<TCommand, TEntity>
    where TCommand : class
    where TEntity : IEntity
{
    /// <summary>
    /// Executes the entity modification.
    /// </summary>
    /// <param name="entity">The existing entity to modify.</param>
    /// <param name="command">The data used to modify the entity.</param>
    /// <returns>The modified entity.</returns>
    public abstract TEntity Execute(TEntity entity, TCommand command);

    /// <summary>
    /// Executes the modification asynchronously by awaiting the entity task.
    /// </summary>
    /// <param name="entityTask">A task that resolves to the entity to modify.</param>
    /// <param name="command">The data used to modify the entity.</param>
    /// <returns>The modified entity.</returns>
    public async Task<TEntity> ExecuteAsync(Task<TEntity> entityTask, TCommand command)
    {
        var entity = await entityTask;
        return Execute(entity, command);
    }
}
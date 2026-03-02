namespace Fudie.Domain;

/// <summary>
/// Marker interface for all domain entities.
/// </summary>
public interface IEntity;

/// <summary>
/// Typed entity interface that exposes a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<TId> : IEntity where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    TId Id { get; }
}

/// <summary>
/// Base class for domain entities with equality based on <see cref="Id"/>.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <param name="id">The unique identifier for this entity.</param>
public abstract class Entity<TId>(TId id) : IEntity<TId> where TId : notnull
{
    /// <summary>
    /// Gets or initializes the unique identifier of the entity.
    /// </summary>
    public TId Id { get; init; } = id;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is Entity<TId> entity)
        {
            return EqualityComparer<TId>.Default.Equals(entity.Id, Id);
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    /// <summary>
    /// Determines whether two entities are equal by comparing their identifiers.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(left.Id, right.Id);
    }

    /// <summary>
    /// Determines whether two entities are not equal by comparing their identifiers.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
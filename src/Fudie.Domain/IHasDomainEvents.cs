namespace Fudie.Domain;

/// <summary>
/// Exposes a collection of domain events and the ability to clear them.
/// Implemented by <see cref="AggregateRoot{TId}"/> so that infrastructure
/// components (e.g. <c>FudieDbContext</c>) can collect and dispatch events
/// regardless of the aggregate's identifier type.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events that have been raised by this aggregate.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Removes all pending domain events.
    /// </summary>
    void ClearDomainEvents();
}

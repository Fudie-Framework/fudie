namespace Fudie.Domain;

/// <summary>
/// Base class for aggregate roots. Extends <see cref="Entity{TId}"/> with a
/// domain events collection exposed through <see cref="IHasDomainEvents"/>.
/// </summary>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
/// <param name="id">The unique identifier for this aggregate root.</param>
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id), IHasDomainEvents where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registers a new domain event to be dispatched when the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc/>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

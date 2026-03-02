namespace Fudie.Infrastructure;

/// <summary>
/// Abstract base <see cref="DbContext"/> that implements
/// <see cref="IEntityLookup"/>, <see cref="IQuery"/>, <see cref="IChangeTracker"/>,
/// and <see cref="IUnitOfWork"/>.
/// Provides default <c>Query&lt;T&gt;()</c> with <c>AsNoTracking</c>,
/// automatically ignores the <c>DomainEvents</c> property in <c>OnModelCreating</c>,
/// and collects/clears domain events on <c>SaveChangesAsync</c>.
/// </summary>
/// <param name="options">The DbContext configuration options.</param>
public abstract class FudieDbContext(DbContextOptions options)
    : DbContext(options), IEntityLookup, IQuery, IChangeTracker, IUnitOfWork
{
    /// <inheritdoc/>
    public IQueryable<T> Query<T>() where T : class, IEntity
        => Set<T>().AsQueryable().AsNoTracking();

    /// <summary>
    /// Configures the model. Automatically ignores the <c>DomainEvents</c> property
    /// on every entity that implements <see cref="IHasDomainEvents"/>.
    /// Override this method and call <c>base.OnModelCreating(modelBuilder)</c> to keep this behavior.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore(nameof(IHasDomainEvents.DomainEvents));
            }
        }
    }

    /// <summary>
    /// Saves all changes, collecting and clearing domain events from tracked
    /// <see cref="IHasDomainEvents"/> entities before persisting.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        // TODO: dispatch domainEvents via Fudie.PubSub (outbox pattern ready)

        return await base.SaveChangesAsync(cancellationToken);
    }
}

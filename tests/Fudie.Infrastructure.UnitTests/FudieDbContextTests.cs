namespace Fudie.Infrastructure.UnitTests;

public class FudieDbContextTests : IDisposable
{
    private record TestDomainEvent(DateTime OccurredAt) : IDomainEvent;

    private class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public string Name { get; set; } = string.Empty;
    }

    private class SimpleEntity(Guid id) : Entity<Guid>(id)
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : FudieDbContext(options)
    {
        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
        public DbSet<SimpleEntity> Entities => Set<SimpleEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestAggregate>().HasKey(a => a.Id);
            modelBuilder.Entity<SimpleEntity>().HasKey(e => e.Id);
        }
    }

    private readonly TestDbContext _context;

    public FudieDbContextTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void Query_ShouldReturnQueryable()
    {
        var query = _context.Query<SimpleEntity>();

        query.Should().NotBeNull();
        query.Should().BeAssignableTo<IQueryable<SimpleEntity>>();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEventsFromTrackedAggregates()
    {
        var aggregate = new TestAggregate(Guid.NewGuid()) { Name = "Test" };
        aggregate.AddDomainEvent(new TestDomainEvent(DateTime.UtcNow));
        aggregate.AddDomainEvent(new TestDomainEvent(DateTime.UtcNow));
        _context.Aggregates.Add(aggregate);

        await _context.SaveChangesAsync();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntities()
    {
        var entity = new SimpleEntity(Guid.NewGuid()) { Value = "Hello" };
        _context.Entities.Add(entity);

        var result = await _context.SaveChangesAsync();

        result.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoEvents_ShouldStillSave()
    {
        var aggregate = new TestAggregate(Guid.NewGuid()) { Name = "NoEvents" };
        _context.Aggregates.Add(aggregate);

        var result = await _context.SaveChangesAsync();

        result.Should().Be(1);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void OnModelCreating_ShouldIgnoreDomainEventsProperty()
    {
        var entityType = _context.Model.FindEntityType(typeof(TestAggregate));

        entityType.Should().NotBeNull();
        var domainEventsProperty = entityType!.FindProperty(nameof(IHasDomainEvents.DomainEvents));
        domainEventsProperty.Should().BeNull();
    }

    [Fact]
    public void FudieDbContext_ShouldImplementIQuery()
    {
        _context.Should().BeAssignableTo<IQuery>();
    }

    [Fact]
    public void FudieDbContext_ShouldImplementIEntityLookup()
    {
        _context.Should().BeAssignableTo<IEntityLookup>();
    }

    [Fact]
    public void FudieDbContext_ShouldImplementIUnitOfWork()
    {
        _context.Should().BeAssignableTo<IUnitOfWork>();
    }

    [Fact]
    public void FudieDbContext_ShouldImplementIChangeTracker()
    {
        _context.Should().BeAssignableTo<IChangeTracker>();
    }
}

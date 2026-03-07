namespace Fudie.Infrastructure.UnitTests;

public class IEntityLookupTests : IDisposable
{
    private class RelatedEntity(Guid id) : Entity<Guid>(id)
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestEntity(Guid id) : Entity<Guid>(id)
    {
        public string Name { get; set; } = string.Empty;
        public RelatedEntity? Related { get; set; }
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : FudieDbContext(options), IEntityLookup
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<RelatedEntity>().HasKey(e => e.Id);
        }
    }

    private readonly TestDbContext _context;

    public IEntityLookupTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TestDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetRequiredAsync_WithExistingEntity_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "Found" });
        await _context.SaveChangesAsync();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id);

        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Name.Should().Be("Found");
    }

    [Fact]
    public async Task GetRequiredAsync_WithNonExistentEntity_ShouldThrowKeyNotFoundException()
    {
        IEntityLookup lookup = _context;

        var act = () => lookup.GetRequiredAsync<TestEntity, Guid>(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*TestEntity*not found*");
    }

    [Fact]
    public async Task GetRequiredAsync_WithTrackingFalse_ShouldReturnUntrackedEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "Untracked" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id, tracking: false);

        result.Should().NotBeNull();
        _context.Entry(result).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetRequiredAsync_WithTrackingTrue_ShouldReturnTrackedEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "Tracked" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id, tracking: true);

        result.Should().NotBeNull();
        _context.Entry(result).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetRequiredAsync_WithIncludeProperty_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        var related = new RelatedEntity(Guid.NewGuid()) { Value = "Included" };
        _context.TestEntities.Add(new TestEntity(id) { Name = "WithInclude", Related = related });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id, includeProperties: "Related");

        result.Should().NotBeNull();
        result.Related.Should().NotBeNull();
        result.Related!.Value.Should().Be("Included");
    }

    [Fact]
    public async Task GetRequiredAsync_WithMultipleIncludeProperties_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        var related = new RelatedEntity(Guid.NewGuid()) { Value = "Multi" };
        _context.TestEntities.Add(new TestEntity(id) { Name = "MultiInclude", Related = related });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id, includeProperties: ["Related", "Related"]);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequiredAsync_WithNoIncludeProperties_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "NoIncludes" });
        await _context.SaveChangesAsync();

        IEntityLookup lookup = _context;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(id);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRequiredAsync_NullableOverload_WithNullId_ShouldReturnNull()
    {
        IEntityLookup lookup = _context;
        Guid? nullId = null;

        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(nullId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRequiredAsync_NullableOverload_WithExistingId_ShouldReturnEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "Optional" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        Guid? nullableId = id;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(nullableId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Optional");
    }

    [Fact]
    public async Task GetRequiredAsync_NullableOverload_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        IEntityLookup lookup = _context;
        Guid? nullableId = Guid.NewGuid();

        var act = () => lookup.GetRequiredAsync<TestEntity, Guid>(nullableId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetRequiredAsync_NullableOverload_WithTrackingFalse_ShouldReturnDetachedEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "OptUntracked" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        Guid? nullableId = id;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(nullableId, tracking: false);

        result.Should().NotBeNull();
        _context.Entry(result!).State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public async Task GetRequiredAsync_NullableOverload_WithTrackingTrue_ShouldReturnTrackedEntity()
    {
        var id = Guid.NewGuid();
        _context.TestEntities.Add(new TestEntity(id) { Name = "OptTracked" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        IEntityLookup lookup = _context;
        Guid? nullableId = id;
        var result = await lookup.GetRequiredAsync<TestEntity, Guid>(nullableId, tracking: true);

        result.Should().NotBeNull();
        _context.Entry(result!).State.Should().Be(EntityState.Unchanged);
    }
}

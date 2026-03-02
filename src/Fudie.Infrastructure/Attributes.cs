namespace Fudie.Infrastructure;

/// <summary>
/// Specifies the navigation properties to include (eager loading) in repository queries.
/// Paths are validated at compile-time by the Source Generator for type-safety.
/// </summary>
/// <typeparam name="TEntity">The root entity type of the repository.</typeparam>
/// <remarks>
/// This attribute allows declarative include configuration using dot notation,
/// identical to EF Core Include/ThenInclude syntax.
/// Can be applied multiple times to the same interface.
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class IncludeAttribute<TEntity> : Attribute where TEntity : class
{
    /// <summary>
    /// Gets the navigation paths to include using dot notation.
    /// </summary>
    public string[] Paths { get; }

    /// <summary>
    /// Gets or sets whether to use AsSplitQuery() for this specific include.
    /// </summary>
    public bool AsSplitQuery { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="IncludeAttribute{TEntity}"/> with the specified paths.
    /// </summary>
    /// <param name="paths">One or more navigation paths using dot notation.</param>
    public IncludeAttribute(params string[] paths)
    {
        ValidatePaths(paths);
        Paths = paths;        
    }

    private static void ValidatePaths(string[] paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (paths.Length == 0)
            throw new ArgumentException("At least one path is required.", nameof(paths));

        for (int i = 0; i < paths.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(paths[i]))
                throw new ArgumentException($"Path at index {i} cannot be null or whitespace.", nameof(paths));
        }
    }
}

/// <summary>
/// Specifies whether repository queries should use Entity Framework Core change tracking.
/// </summary>
/// <remarks>
/// Default behavior when not specified:
/// IGet only → tracking = false (optimized for reads),
/// IUpdate → tracking = true (required for detecting changes),
/// IRemove → tracking = true (required for deleting).
/// </remarks>
/// <param name="enabled"><c>true</c> to enable change tracking; <c>false</c> to disable it.</param>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class TrackingAttribute(bool enabled = true) : Attribute
{
    /// <summary>
    /// Gets whether change tracking is enabled.
    /// </summary>
    public bool Enabled { get; } = enabled;
}

/// <summary>
/// Indicates that repository queries should execute without change tracking (AsNoTracking).
/// Semantic alias for <c>[Tracking(false)]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AsNoTrackingAttribute : TrackingAttribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="AsNoTrackingAttribute"/>.
    /// </summary>
    public AsNoTrackingAttribute() : base(false)
    {
    }
}

/// <summary>
/// Indicates that repository queries should use split queries (AsSplitQuery)
/// to avoid cartesian explosion when including multiple collections.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class AsSplitQueryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="AsSplitQueryAttribute"/>.
    /// </summary>
    public AsSplitQueryAttribute()
    {
    }
}

/// <summary>
/// Indicates that repository queries should ignore EF Core global query filters.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreQueryFiltersAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="IgnoreQueryFiltersAttribute"/>.
    /// </summary>
    public IgnoreQueryFiltersAttribute()
    {
    }
}

/// <summary>
/// Marks an interface for the Source Generator to produce a repository implementation
/// without requiring inheritance from IGet, IAdd, IUpdate, or IRemove.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class GenerateRepositoryAttribute<TEntity, TId> : Attribute
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="GenerateRepositoryAttribute{TEntity, TId}"/>.
    /// </summary>
    public GenerateRepositoryAttribute()
    {
    }
}

/// <summary>
/// Marks an interface for the Source Generator to produce a repository implementation.
/// Uses <see cref="Guid"/> as the default identifier type.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class GenerateRepositoryAttribute<TEntity> : Attribute
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="GenerateRepositoryAttribute{TEntity}"/>.
    /// </summary>
    public GenerateRepositoryAttribute()
    {
    }
}

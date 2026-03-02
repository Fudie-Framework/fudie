namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Type of query to execute.
/// </summary>
public enum QueryType
{
    /// <summary>
    /// Query that returns entities (FindBy, FindFirstBy, FindTopNBy).
    /// </summary>
    Find,

    /// <summary>
    /// Query that counts records (CountBy).
    /// </summary>
    Count,

    /// <summary>
    /// Query that checks existence (ExistsBy).
    /// </summary>
    Exists,

    /// <summary>
    /// Delete operation (DeleteBy).
    /// </summary>
    Delete
}

/// <summary>
/// Supported comparison operators.
/// </summary>
public enum Operator
{
    /// <summary>
    /// Equality (=).
    /// </summary>
    Equal,

    /// <summary>
    /// Inequality (!=).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Less than (&lt;).
    /// </summary>
    LessThan,

    /// <summary>
    /// Less than or equal (&lt;=).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Greater than (&gt;).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater than or equal (&gt;=).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Between two values (&gt;= min &amp;&amp; &lt;= max).
    /// </summary>
    Between,

    /// <summary>
    /// Value is in a collection (Contains).
    /// </summary>
    In,

    /// <summary>
    /// Value is not in a collection (!Contains).
    /// </summary>
    NotIn,

    /// <summary>
    /// Starts with (StartsWith).
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with (EndsWith).
    /// </summary>
    EndsWith,

    /// <summary>
    /// Contains (Contains).
    /// </summary>
    Contains,

    /// <summary>
    /// SQL LIKE pattern.
    /// </summary>
    Like,

    /// <summary>
    /// Is null (== null).
    /// </summary>
    IsNull,

    /// <summary>
    /// Is not null (!= null).
    /// </summary>
    IsNotNull,

    /// <summary>
    /// Is true (== true).
    /// </summary>
    True,

    /// <summary>
    /// Is false (== false).
    /// </summary>
    False
}

/// <summary>
/// Filter condition in a query.
/// </summary>
/// <param name="Property">Name of the property to filter on.</param>
/// <param name="Op">Operator to apply.</param>
/// <param name="Or">If true, joined with OR instead of AND.</param>
/// <param name="IgnoreCase">If true, apply case-insensitive comparison (strings only).</param>
public record Condition(
    string Property,
    Operator Op,
    bool Or = false,
    bool IgnoreCase = false
);

/// <summary>
/// Ordering clause in a query.
/// </summary>
/// <param name="Property">Name of the property to order by.</param>
/// <param name="Descending">If true, descending order; if false, ascending.</param>
public record OrderBy(
    string Property,
    bool Descending = false
);

/// <summary>
/// Query parsed from a method name.
/// </summary>
public record ParsedQuery
{
    /// <summary>
    /// Type of query (Find, Count, Exists, Delete).
    /// </summary>
    public QueryType Type { get; init; }

    /// <summary>
    /// If true, return only the first result (FindFirstBy).
    /// </summary>
    public bool First { get; init; }

    /// <summary>
    /// Number of results to limit to (FindTopNBy).
    /// </summary>
    public int? Top { get; init; }

    /// <summary>
    /// List of filter conditions.
    /// </summary>
    public List<Condition> Conditions { get; init; } = new();

    /// <summary>
    /// List of ordering clauses.
    /// </summary>
    public List<OrderBy> OrderBy { get; init; } = new();
}

/// <summary>
/// Result of parsing a method name.
/// </summary>
public record ParseResult
{
    /// <summary>
    /// If true, parsing was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Parsed query (only when Success = true).
    /// </summary>
    public ParsedQuery? Query { get; init; }

    /// <summary>
    /// Error message (only when Success = false).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Position in the method name where the error occurred.
    /// </summary>
    public int? ErrorPosition { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ParseResult Ok(ParsedQuery query) => new()
    {
        Success = true,
        Query = query
    };

    /// <summary>
    /// Creates a result with an error.
    /// </summary>
    public static ParseResult Error(string message, int? position = null) => new()
    {
        Success = false,
        ErrorMessage = message,
        ErrorPosition = position
    };
}

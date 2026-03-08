namespace Fudie.Validation;

/// <summary>
/// Represents a uniquely identifiable validation error with its metadata.
/// </summary>
/// <param name="Aggregate">The aggregate name (e.g. "Customer").</param>
/// <param name="Property">The property name (e.g. "Name").</param>
/// <param name="Rule">The rule name (e.g. "Required", "MaxLength").</param>
/// <param name="Message">The message template with placeholders (e.g. "{PropertyName} is required").</param>
/// <param name="Category">Whether the rule is FluentValidation (UX) or Guard (backend only). Defaults to FluentValidation.</param>
public record ErrorCode(
    string Aggregate,
    string Property,
    string Rule,
    string Message,
    ErrorCodeCategory Category = ErrorCodeCategory.FluentValidation
)
{
    /// <summary>
    /// Gets the unique error code in the format "Aggregate.Property.Rule".
    /// </summary>
    public string Code => $"{Aggregate}.{Property}.{Rule}";
}

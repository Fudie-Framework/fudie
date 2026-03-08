namespace Fudie.Validation;

/// <summary>
/// Categorizes error codes by their validation scope.
/// </summary>
public enum ErrorCodeCategory
{
    /// <summary>
    /// Structural and format validation rules.
    /// Exported to Angular as UX validators on form fields.
    /// </summary>
    FluentValidation = 1,

    /// <summary>
    /// Business logic rules enforced only on the server.
    /// Displayed in the Angular ValidationSummary component.
    /// </summary>
    Guard = 2
}

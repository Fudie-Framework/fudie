namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Diagnostic definitions for the Query Method Generator.
/// </summary>
public static class Diagnostics
{
    private const string Category = "Fudie.QueryMethod";

    /// <summary>
    /// REPO001: Property does not exist on the entity.
    /// </summary>
    public static DiagnosticDescriptor PropertyNotFound { get; } = new(
        id: "REPO001",
        title: "Property not found",
        messageFormat: "'{0}' does not exist on '{1}'.{2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The property specified in the method name does not exist on the entity."
    );

    /// <summary>
    /// REPO002: Parameter type is incompatible with the property type.
    /// </summary>
    public static DiagnosticDescriptor TypeMismatch { get; } = new(
        id: "REPO002",
        title: "Parameter type mismatch",
        messageFormat: "Parameter '{0}' is '{1}' but '{2}' is '{3}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The parameter type is not compatible with the property type."
    );

    /// <summary>
    /// REPO003: Required parameter is missing.
    /// </summary>
    public static DiagnosticDescriptor MissingParameter { get; } = new(
        id: "REPO003",
        title: "Missing parameter",
        messageFormat: "Missing parameter for '{0}' in '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method requires an additional parameter based on the conditions in its name."
    );

    /// <summary>
    /// REPO004: Incorrect number of parameters.
    /// </summary>
    public static DiagnosticDescriptor WrongParameterCount { get; } = new(
        id: "REPO004",
        title: "Wrong parameter count",
        messageFormat: "'{0}' has {1} parameters but {2} were expected",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method has more parameters than expected based on its name."
    );

    /// <summary>
    /// REPO005: Incorrect return type.
    /// </summary>
    public static DiagnosticDescriptor WrongReturnType { get; } = new(
        id: "REPO005",
        title: "Wrong return type",
        messageFormat: "'{0}' must return '{1}', not '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method's return type does not match the expected type based on its prefix."
    );

    /// <summary>
    /// REPO006: Operator is incompatible with the property type.
    /// </summary>
    public static DiagnosticDescriptor IncompatibleOperator { get; } = new(
        id: "REPO006",
        title: "Incompatible operator",
        messageFormat: "Operator '{0}' is not valid for type '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The specified operator is not compatible with the property type."
    );

    /// <summary>
    /// REPO007: Error parsing the method name.
    /// </summary>
    public static DiagnosticDescriptor ParseError { get; } = new(
        id: "REPO007",
        title: "Parse error",
        messageFormat: "Could not parse '{0}': {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The method name does not follow the expected convention or contains syntax errors."
    );

    /// <summary>
    /// Creates a diagnostic for a property that was not found.
    /// </summary>
    /// <param name="propertyName">Name of the property that does not exist.</param>
    /// <param name="entityName">Name of the entity.</param>
    /// <param name="suggestion">Optional correction suggestion.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreatePropertyNotFound(
        string propertyName,
        string entityName,
        string? suggestion,
        Location? location)
    {
        var suggestionText = suggestion != null ? $" Did you mean '{suggestion}'?" : "";
        return Diagnostic.Create(
            PropertyNotFound,
            location,
            propertyName,
            entityName,
            suggestionText
        );
    }

    /// <summary>
    /// Creates a diagnostic for a type mismatch.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="parameterType">Type of the parameter.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="propertyType">Type of the property.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateTypeMismatch(
        string parameterName,
        string parameterType,
        string propertyName,
        string propertyType,
        Location? location)
    {
        return Diagnostic.Create(
            TypeMismatch,
            location,
            parameterName,
            parameterType,
            propertyName,
            propertyType
        );
    }

    /// <summary>
    /// Creates a diagnostic for a missing parameter.
    /// </summary>
    /// <param name="propertyName">Name of the property that requires a parameter.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateMissingParameter(
        string propertyName,
        string methodName,
        Location? location)
    {
        return Diagnostic.Create(
            MissingParameter,
            location,
            propertyName,
            methodName
        );
    }

    /// <summary>
    /// Creates a diagnostic for an incorrect parameter count.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="actualCount">Actual number of parameters.</param>
    /// <param name="expectedCount">Expected number of parameters.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateWrongParameterCount(
        string methodName,
        int actualCount,
        int expectedCount,
        Location? location)
    {
        return Diagnostic.Create(
            WrongParameterCount,
            location,
            methodName,
            actualCount,
            expectedCount
        );
    }

    /// <summary>
    /// Creates a diagnostic for an incorrect return type.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="expectedType">Expected return type.</param>
    /// <param name="actualType">Actual return type.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateWrongReturnType(
        string methodName,
        string expectedType,
        string actualType,
        Location? location)
    {
        return Diagnostic.Create(
            WrongReturnType,
            location,
            methodName,
            expectedType,
            actualType
        );
    }

    /// <summary>
    /// Creates a diagnostic for an incompatible operator.
    /// </summary>
    /// <param name="operatorName">Name of the operator.</param>
    /// <param name="propertyType">Type of the property.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateIncompatibleOperator(
        string operatorName,
        string propertyType,
        Location? location)
    {
        return Diagnostic.Create(
            IncompatibleOperator,
            location,
            operatorName,
            propertyType
        );
    }

    /// <summary>
    /// Creates a diagnostic for a parse error.
    /// </summary>
    /// <param name="methodName">Name of the method that failed to parse.</param>
    /// <param name="errorMessage">Description of the error.</param>
    /// <param name="location">Location in the source code.</param>
    public static Diagnostic CreateParseError(
        string methodName,
        string errorMessage,
        Location? location)
    {
        return Diagnostic.Create(
            ParseError,
            location,
            methodName,
            errorMessage
        );
    }
}

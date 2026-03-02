namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Validates parsed queries against method signatures and entity types.
/// </summary>
public class QueryValidator
{
    /// <summary>
    /// Validates a parsed query against a method and entity type.
    /// </summary>
    /// <param name="query">Parsed query.</param>
    /// <param name="method">Method to validate.</param>
    /// <param name="entityType">Entity type.</param>
    /// <param name="location">Source code location.</param>
    /// <returns>List of diagnostics (empty if no errors).</returns>
    public List<Diagnostic> Validate(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location)
    {
        var diagnostics = new List<Diagnostic>();

        foreach (var condition in query.Conditions)
        {
            ValidatePropertyExists(condition.Property, entityType, location, diagnostics);
        }

        foreach (var orderBy in query.OrderBy)
        {
            ValidatePropertyExists(orderBy.Property, entityType, location, diagnostics);
        }

        foreach (var condition in query.Conditions)
        {
            var property = FindProperty(condition.Property, entityType);
            if (property != null)
            {
                ValidateOperatorCompatibility(condition.Op, property, location, diagnostics);
            }
        }

        ValidateParameterCount(query, method, entityType, location, diagnostics);

        ValidateReturnType(query, method, entityType, location, diagnostics);

        ValidateParameterTypes(query, method, entityType, location, diagnostics);

        return diagnostics;
    }

    /// <summary>
    /// Validates that a property exists on the entity type.
    /// </summary>
    private void ValidatePropertyExists(
        string propertyName,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var property = FindProperty(propertyName, entityType);
        if (property == null)
        {
            var suggestion = FindSimilarPropertyName(propertyName, entityType);
            diagnostics.Add(Diagnostics.CreatePropertyNotFound(
                propertyName,
                entityType.Name,
                suggestion,
                location
            ));
        }
    }

    /// <summary>
    /// Validates that an operator is compatible with the property type.
    /// </summary>
    private void ValidateOperatorCompatibility(
        Operator op,
        IPropertySymbol property,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var propertyType = property.Type;
        var typeName = propertyType.ToDisplayString();

        switch (op)
        {
            case Operator.LessThan:
            case Operator.LessThanOrEqual:
            case Operator.GreaterThan:
            case Operator.GreaterThanOrEqual:
            case Operator.Between:
                if (!IsNumericOrDateType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;

            case Operator.StartsWith:
            case Operator.EndsWith:
            case Operator.Contains:
            case Operator.Like:
                if (!IsStringType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;

            case Operator.True:
            case Operator.False:
                if (!IsBooleanType(propertyType))
                {
                    diagnostics.Add(Diagnostics.CreateIncompatibleOperator(
                        op.ToString(),
                        typeName,
                        location
                    ));
                }
                break;
        }
    }

    /// <summary>
    /// Validates the parameter count of the method against the expected count from the query.
    /// </summary>
    private void ValidateParameterCount(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var expectedCount = CalculateExpectedParameterCount(query, entityType);
        var actualCount = method.Parameters.Length;

        if (actualCount < expectedCount)
        {
            diagnostics.Add(Diagnostics.CreateMissingParameter(
                "parameter",
                method.Name,
                location
            ));
        }
        else if (actualCount > expectedCount)
        {
            diagnostics.Add(Diagnostics.CreateWrongParameterCount(
                method.Name,
                actualCount,
                expectedCount,
                location
            ));
        }
    }

    /// <summary>
    /// Calculates the expected parameter count based on the query conditions.
    /// </summary>
    private int CalculateExpectedParameterCount(ParsedQuery query, INamedTypeSymbol entityType)
    {
        var count = 0;

        foreach (var condition in query.Conditions)
        {
            count += GetParameterCountForOperator(condition.Op);
        }

        return count;
    }

    /// <summary>
    /// Returns the number of parameters required by an operator.
    /// </summary>
    private int GetParameterCountForOperator(Operator op)
    {
        return op switch
        {
            Operator.Between => 2,
            Operator.IsNull => 0,
            Operator.IsNotNull => 0,
            Operator.True => 0,
            Operator.False => 0,
            _ => 1
        };
    }

    /// <summary>
    /// Validates the return type of the method against the expected return type for the query.
    /// </summary>
    private void ValidateReturnType(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var returnType = method.ReturnType;

        if (returnType is not INamedTypeSymbol namedReturnType ||
            namedReturnType.Name != "Task")
        {
            diagnostics.Add(Diagnostics.CreateWrongReturnType(
                method.Name,
                GetExpectedReturnType(query, entityType),
                returnType.ToDisplayString(),
                location
            ));
            return;
        }

        if (namedReturnType.TypeArguments.Length == 0)
        {
            if (query.Type != QueryType.Delete)
            {
                diagnostics.Add(Diagnostics.CreateWrongReturnType(
                    method.Name,
                    GetExpectedReturnType(query, entityType),
                    returnType.ToDisplayString(),
                    location
                ));
            }
            return;
        }

        var taskArgument = namedReturnType.TypeArguments[0];

        switch (query.Type)
        {
            case QueryType.Find:
                var isSingleElement = IsNullableOf(taskArgument, entityType) ||
                                     SymbolEqualityComparer.Default.Equals(taskArgument, entityType);
                var isList = IsListOf(taskArgument, entityType) || IsEnumerableOf(taskArgument, entityType);

                if (!isSingleElement && !isList)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        query.First || query.Top == 1
                            ? $"Task<{entityType.Name}?>"
                            : $"Task<List<{entityType.Name}>> or Task<{entityType.Name}?>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Count:
                if (taskArgument.SpecialType != SpecialType.System_Int32 &&
                    taskArgument.SpecialType != SpecialType.System_Int64)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<int>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Exists:
                if (taskArgument.SpecialType != SpecialType.System_Boolean)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<bool>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;

            case QueryType.Delete:
                if (taskArgument.SpecialType != SpecialType.System_Int32)
                {
                    diagnostics.Add(Diagnostics.CreateWrongReturnType(
                        method.Name,
                        "Task<int>",
                        returnType.ToDisplayString(),
                        location
                    ));
                }
                break;
        }
    }

    /// <summary>
    /// Returns the expected return type as a string for a given query and entity type.
    /// </summary>
    internal string GetExpectedReturnType(ParsedQuery query, INamedTypeSymbol entityType)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => $"Task<{entityType.Name}?>",
            QueryType.Find => $"Task<List<{entityType.Name}>>",
            QueryType.Count => "Task<int>",
            QueryType.Exists => "Task<bool>",
            QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Validates the types of method parameters against their corresponding query conditions.
    /// </summary>
    private void ValidateParameterTypes(
        ParsedQuery query,
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        Location? location,
        List<Diagnostic> diagnostics)
    {
        var parameterIndex = 0;

        foreach (var condition in query.Conditions)
        {
            var property = FindProperty(condition.Property, entityType);
            if (property == null)
                continue;

            var paramCount = GetParameterCountForOperator(condition.Op);

            for (int i = 0; i < paramCount; i++)
            {
                if (parameterIndex >= method.Parameters.Length)
                    break;

                var parameter = method.Parameters[parameterIndex];
                var propertyType = property.Type;

                if (condition.Op == Operator.In || condition.Op == Operator.NotIn)
                {
                    if (!IsEnumerableOf(parameter.Type, propertyType))
                    {
                        diagnostics.Add(Diagnostics.CreateTypeMismatch(
                            parameter.Name,
                            parameter.Type.ToDisplayString(),
                            property.Name,
                            $"IEnumerable<{propertyType.ToDisplayString()}>",
                            location
                        ));
                    }
                }
                else
                {
                    if (!AreTypesCompatible(parameter.Type, propertyType))
                    {
                        diagnostics.Add(Diagnostics.CreateTypeMismatch(
                            parameter.Name,
                            parameter.Type.ToDisplayString(),
                            property.Name,
                            propertyType.ToDisplayString(),
                            location
                        ));
                    }
                }

                parameterIndex++;
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// Finds a property on the entity type, including inherited properties.
    /// </summary>
    private IPropertySymbol? FindProperty(string propertyName, INamedTypeSymbol entityType)
    {
        var currentType = entityType;
        while (currentType != null)
        {
            var property = currentType.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
                return property;

            currentType = currentType.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Finds a property with a similar name using Levenshtein distance.
    /// </summary>
    private string? FindSimilarPropertyName(string propertyName, INamedTypeSymbol entityType)
    {
        var properties = GetAllProperties(entityType);
        var minDistance = int.MaxValue;
        string? bestMatch = null;

        foreach (var property in properties)
        {
            var distance = CalculateLevenshteinDistance(propertyName, property.Name);
            if (distance <= 3 && distance < minDistance)
            {
                minDistance = distance;
                bestMatch = property.Name;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Returns all properties of the entity type, including inherited ones.
    /// </summary>
    private List<IPropertySymbol> GetAllProperties(INamedTypeSymbol entityType)
    {
        var properties = new List<IPropertySymbol>();
        var currentType = entityType;

        while (currentType != null)
        {
            properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>());
            currentType = currentType.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    internal int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return target?.Length ?? 0;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= targetLength; j++)
            distance[0, j] = j;

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                var cost = char.ToLower(source[i - 1]) == char.ToLower(target[j - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost
                );
            }
        }

        return distance[sourceLength, targetLength];
    }

    /// <summary>
    /// Determines whether a type is numeric or a date/time type.
    /// </summary>
    private bool IsNumericOrDateType(ITypeSymbol type)
    {
        var unwrapped = UnwrapNullable(type);

        return unwrapped.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_DateTime => true,
            _ => unwrapped.ToDisplayString() switch
            {
                "System.DateOnly" => true,
                "System.TimeOnly" => true,
                "System.DateTimeOffset" => true,
                _ => false
            }
        };
    }

    /// <summary>
    /// Determines whether a type is string.
    /// </summary>
    private bool IsStringType(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_String;
    }

    /// <summary>
    /// Determines whether a type is boolean.
    /// </summary>
    private bool IsBooleanType(ITypeSymbol type)
    {
        var unwrapped = UnwrapNullable(type);
        return unwrapped.SpecialType == SpecialType.System_Boolean;
    }

    /// <summary>
    /// Determines whether two types are compatible for assignment or comparison.
    /// </summary>
    private bool AreTypesCompatible(ITypeSymbol parameterType, ITypeSymbol propertyType)
    {
        if (SymbolEqualityComparer.Default.Equals(parameterType, propertyType))
            return true;

        var unwrappedParam = UnwrapNullable(parameterType);
        var unwrappedProp = UnwrapNullable(propertyType);

        if (SymbolEqualityComparer.Default.Equals(unwrappedParam, unwrappedProp))
            return true;

        if (IsNumericType(unwrappedParam) && IsNumericType(unwrappedProp))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether a type is a numeric type.
    /// </summary>
    private bool IsNumericType(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            _ => false
        };
    }

    /// <summary>
    /// Unwraps a nullable type to its underlying type.
    /// </summary>
    private ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        return type;
    }

    /// <summary>
    /// Determines whether a type is T? (nullable of another type).
    /// </summary>
    private bool IsNullableOf(ITypeSymbol type, ITypeSymbol innerType)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], innerType);
        }

        if (type.IsReferenceType)
        {
            return SymbolEqualityComparer.Default.Equals(type, innerType);
        }

        return false;
    }

    /// <summary>
    /// Determines whether a type is List&lt;T&gt; of the given element type.
    /// </summary>
    private bool IsListOf(ITypeSymbol type, ITypeSymbol elementType)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.Name == "List" &&
            namedType.TypeArguments.Length == 1)
        {
            return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], elementType);
        }

        return false;
    }

    /// <summary>
    /// Determines whether a type is IEnumerable&lt;T&gt; of the given element type,
    /// either directly or through an implemented interface.
    /// </summary>
    private bool IsEnumerableOf(ITypeSymbol type, ITypeSymbol elementType)
    {
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
            {
                return SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], elementType);
            }

            foreach (var iface in namedType.AllInterfaces)
            {
                if (iface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>" &&
                    iface.TypeArguments.Length == 1)
                {
                    return SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], elementType);
                }
            }
        }

        return false;
    }

    #endregion
}

namespace Fudie.Generator;

/// <summary>
/// Validates navigation paths for Include in Entity Framework.
/// </summary>
internal static class PathValidator
{
    /// <summary>
    /// Validation result for an include path.
    /// </summary>
    public record IncludePathInfo(
        string OriginalPath,
        string[] Segments,
        SegmentInfo[] SegmentDetails,
        bool IsValid,
        string? ErrorMessage,
        Location? Location
    );

    /// <summary>
    /// Detailed information about a single path segment.
    /// </summary>
    public record SegmentInfo(
        string PropertyName,
        INamedTypeSymbol ContainingType,
        IPropertySymbol PropertySymbol,
        ITypeSymbol PropertyType,
        INamedTypeSymbol? ElementType,
        bool IsCollection
    );

    /// <summary>
    /// Validates a complete navigation path.
    /// </summary>
    /// <param name="path">Path to validate (e.g. "Orders.OrderItems.Product").</param>
    /// <param name="rootEntityType">Root entity type.</param>
    /// <param name="compilation">Compilation used to resolve types.</param>
    /// <param name="location">Source location (optional).</param>
    /// <returns>Validation result for the path.</returns>
    public static IncludePathInfo ValidatePath(
        string path,
        INamedTypeSymbol rootEntityType,
        Compilation compilation,
        Location? location = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new IncludePathInfo(
                OriginalPath: path ?? "",
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Include path cannot be null or whitespace.",
                Location: location
            );
        }

        if (rootEntityType == null)
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Root entity type cannot be null.",
                Location: location
            );
        }

        if (compilation == null)
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: Array.Empty<string>(),
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Compilation cannot be null.",
                Location: location
            );
        }

        var segments = path.Split('.')
            .Select(s => s.Trim())
            .ToArray();

        if (segments.Any(string.IsNullOrWhiteSpace))
        {
            return new IncludePathInfo(
                OriginalPath: path,
                Segments: segments,
                SegmentDetails: Array.Empty<SegmentInfo>(),
                IsValid: false,
                ErrorMessage: "Include path contains empty segments. Check for consecutive dots (..), leading dots, or trailing dots.",
                Location: location
            );
        }

        var segmentDetails = new List<SegmentInfo>();
        var currentType = rootEntityType;

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            var property = FindProperty(currentType, segment);

            if (property == null)
            {
                var similarProperty = FindSimilarPropertyName(currentType, segment);
                var suggestion = similarProperty != null
                    ? $" Did you mean '{similarProperty}'?"
                    : "";

                return new IncludePathInfo(
                    OriginalPath: path,
                    Segments: segments,
                    SegmentDetails: segmentDetails.ToArray(),
                    IsValid: false,
                    ErrorMessage: $"Property '{segment}' does not exist on type '{currentType.Name}'.{suggestion}",
                    Location: location
                );
            }

            var (isNavigation, propertyType, elementType, isCollection) =
                GetNavigationElementType(property, compilation);

            if (!isNavigation)
            {
                return new IncludePathInfo(
                    OriginalPath: path,
                    Segments: segments,
                    SegmentDetails: segmentDetails.ToArray(),
                    IsValid: false,
                    ErrorMessage: $"Property '{segment}' on type '{currentType.Name}' is not a navigation property. " +
                                $"It has type '{propertyType.ToDisplayString()}' which is a scalar value. " +
                                $"Only entity types and collections can be included.",
                    Location: location
                );
            }

            segmentDetails.Add(new SegmentInfo(
                PropertyName: segment,
                ContainingType: currentType,
                PropertySymbol: property,
                PropertyType: propertyType,
                ElementType: elementType,
                IsCollection: isCollection
            ));

            currentType = elementType!;
        }

        return new IncludePathInfo(
            OriginalPath: path,
            Segments: segments,
            SegmentDetails: segmentDetails.ToArray(),
            IsValid: true,
            ErrorMessage: null,
            Location: location
        );
    }

    /// <summary>
    /// Finds a property by name on a type.
    /// </summary>
    private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string propertyName)
    {
        return type.GetMembers()
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.Name == propertyName);
    }

    /// <summary>
    /// Determines whether a property is a navigation property and returns its type information.
    /// </summary>
    private static (bool isNavigation, ITypeSymbol propertyType, INamedTypeSymbol? elementType, bool isCollection)
        GetNavigationElementType(IPropertySymbol property, Compilation compilation)
    {
        var propertyType = property.Type;

        if (IsCollectionType(propertyType, out var elementType))
        {
            return (true, propertyType, elementType, true);
        }

        if (propertyType is INamedTypeSymbol namedType && !IsScalarType(namedType, compilation))
        {
            return (true, propertyType, namedType, false);
        }

        return (false, propertyType, null, false);
    }

    /// <summary>
    /// Determines whether a type is a generic collection type.
    /// </summary>
    private static bool IsCollectionType(ITypeSymbol type, out INamedTypeSymbol? elementType)
    {
        elementType = null;

        if (type is not INamedTypeSymbol namedType)
            return false;

        if (namedType.IsGenericType)
        {
            var genericDefinition = namedType.ConstructedFrom.ToDisplayString();

            if (genericDefinition == "System.Collections.Generic.ICollection<T>" ||
                genericDefinition == "System.Collections.Generic.IEnumerable<T>" ||
                genericDefinition == "System.Collections.Generic.IReadOnlyCollection<T>" ||
                genericDefinition == "System.Collections.Generic.IReadOnlyList<T>" ||
                genericDefinition == "System.Collections.Generic.List<T>" ||
                genericDefinition == "System.Collections.Generic.HashSet<T>" ||
                genericDefinition == "System.Collections.Generic.IList<T>")
            {
                elementType = namedType.TypeArguments[0] as INamedTypeSymbol;
                return elementType != null;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a type is a scalar type (primitive, string, decimal, DateTime, etc.).
    /// </summary>
    private static bool IsScalarType(INamedTypeSymbol type, Compilation compilation)
    {
        var typeString = type.ToDisplayString();

        var scalarTypes = new[]
        {
            "string", "System.String",
            "int", "System.Int32",
            "long", "System.Int64",
            "short", "System.Int16",
            "byte", "System.Byte",
            "bool", "System.Boolean",
            "decimal", "System.Decimal",
            "double", "System.Double",
            "float", "System.Single",
            "System.DateTime",
            "System.DateTimeOffset",
            "System.TimeSpan",
            "System.Guid",
            "char", "System.Char"
        };

        return scalarTypes.Contains(typeString) || type.TypeKind == TypeKind.Enum;
    }

    /// <summary>
    /// Finds a property with a similar name using Levenshtein distance.
    /// </summary>
    private static string? FindSimilarPropertyName(INamedTypeSymbol type, string propertyName)
    {
        var properties = type.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(p => p.Name)
            .ToArray();

        var candidates = properties
            .Select(p => (name: p, distance: CalculateLevenshteinDistance(propertyName.ToLowerInvariant(), p.ToLowerInvariant())))
            .Where(x => x.distance <= 2)
            .OrderBy(x => x.distance)
            .ToArray();

        return candidates.FirstOrDefault().name;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    private static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 0 : target.Length;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }

}

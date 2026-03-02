namespace Fudie.Generator;

/// <summary>
/// Source generator that automatically generates repository implementations.
/// </summary>
[Generator]
public class RepositorySourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator pipeline.
    /// </summary>
    /// <param name="context">The initialization context provided by the Roslyn infrastructure.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
        });

        var repositoryInterfaces = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateInterface(node),
                transform: static (ctx, _) => GetRepositoryInterfaceInfo(ctx))
            .Where(static info => info is not null);

        var compilationAndInterfaces = context.CompilationProvider.Combine(repositoryInterfaces.Collect());

        context.RegisterSourceOutput(compilationAndInterfaces,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    /// <summary>
    /// Determines whether the given syntax node is a candidate interface for processing.
    /// </summary>
    /// <param name="node">The syntax node to evaluate.</param>
    /// <returns><see langword="true"/> if the node is an interface declaration; otherwise, <see langword="false"/>.</returns>
    private static bool IsCandidateInterface(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax;
    }

    /// <summary>
    /// Extracts repository interface information from the generator syntax context.
    /// </summary>
    /// <param name="context">The generator syntax context for the candidate node.</param>
    /// <returns>A <see cref="RepositoryInterfaceInfo"/> instance if the interface qualifies; otherwise, <see langword="null"/>.</returns>
    private static RepositoryInterfaceInfo? GetRepositoryInterfaceInfo(GeneratorSyntaxContext context)
    {
        var interfaceDecl = (InterfaceDeclarationSyntax)context.Node;
        var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;

        if (interfaceSymbol == null)
            return null;

        if (interfaceSymbol.ContainingNamespace!.ToDisplayString() == "Fudie.Infrastructure")
            return null;

        var hasGenerateRepositoryAttribute = interfaceSymbol.GetAttributes()
            .Any(attr =>
            {
                var attrName = attr.AttributeClass!.Name;
                return attrName == "GenerateRepositoryAttribute";
            });

        var isRepositoryInterface = interfaceSymbol.AllInterfaces.Any(i =>
        {
            var fullName = i.ConstructedFrom.ToDisplayString();
            return fullName == "Fudie.Infrastructure.IGet<T, ID>" ||
                   fullName == "Fudie.Infrastructure.IAdd<T>" ||
                   fullName == "Fudie.Infrastructure.IUpdate<T, ID>" ||
                   fullName == "Fudie.Infrastructure.IRemove<T, ID>";
        });

        if (!isRepositoryInterface && !hasGenerateRepositoryAttribute)
            return null;

        return new RepositoryInterfaceInfo(interfaceDecl, interfaceSymbol);
    }

    /// <summary>
    /// Executes the source generation pass for all collected repository interfaces.
    /// </summary>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="interfaces">The set of discovered repository interface descriptors.</param>
    /// <param name="context">The source production context used to emit files and diagnostics.</param>
    private static void Execute(
        Compilation compilation,
        ImmutableArray<RepositoryInterfaceInfo> interfaces,
        SourceProductionContext context)
    {
        foreach (var info in interfaces)
        {
            try
            {
                GenerateRepository(compilation, info, context);
            }
            catch (Exception ex)
            {
                var descriptor = new DiagnosticDescriptor(
                    "FUDIE001",
                    "Repository Generation Error",
                    $"Error generating repository for {info.Symbol.Name}: {ex.Message}",
                    "Fudie.Generator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

                context.ReportDiagnostic(Diagnostic.Create(descriptor, info.Syntax.GetLocation()));
            }
        }
    }

    /// <summary>
    /// Generates the repository implementation source file for a single interface.
    /// </summary>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="info">The repository interface descriptor to generate code for.</param>
    /// <param name="context">The source production context used to emit the generated file.</param>
    private static void GenerateRepository(
        Compilation compilation,
        RepositoryInterfaceInfo info,
        SourceProductionContext context)
    {
        var interfaceSymbol = info.Symbol;

        var repoConfig = ExtractRepositoryConfiguration(interfaceSymbol, compilation, context);

        if (repoConfig == null)
            return;

        var code = CodeBuilder.GenerateRepositoryClass(
            className: repoConfig.ClassName,
            namespaceName: repoConfig.Namespace,
            entityTypeName: repoConfig.EntityTypeName,
            idTypeName: repoConfig.IdTypeName,
            config: repoConfig.BuilderConfig);

        var fileName = $"{repoConfig.Namespace}.{repoConfig.ClassName}.g.cs";
        context.AddSource(fileName, code);
    }

    /// <summary>
    /// Extracts all configuration required to generate a repository class from the given interface symbol.
    /// </summary>
    /// <param name="interfaceSymbol">The interface symbol to analyse.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="context">The source production context used to report diagnostics.</param>
    /// <returns>A <see cref="RepositoryConfiguration"/> when extraction succeeds; otherwise, <see langword="null"/>.</returns>
    private static RepositoryConfiguration? ExtractRepositoryConfiguration(
        INamedTypeSymbol interfaceSymbol,
        Compilation compilation,
        SourceProductionContext context)
    {
        var namespaceName = interfaceSymbol.ContainingNamespace!.ToDisplayString() ?? "Generated";

        var interfaceName = interfaceSymbol.Name;
        var baseClassName = interfaceName.StartsWith("I") && interfaceName.Length > 1
            ? interfaceName.Substring(1)
            : interfaceName + "Impl";

        var containingType = interfaceSymbol.ContainingType;
        var className = containingType != null
            ? $"{containingType.Name}_{baseClassName}"
            : baseClassName;

        var baseInterfaces = interfaceSymbol.AllInterfaces;
        bool implementsIGet = false;
        bool implementsIAdd = false;
        bool implementsIUpdate = false;
        bool implementsIRemove = false;

        string? entityTypeName = null;
        string? entityTypeNamespace = null;
        string? idTypeName = null;

        foreach (var baseInterface in baseInterfaces)
        {
            var interfaceFullName = baseInterface.ConstructedFrom.ToDisplayString();

            if (interfaceFullName == "Fudie.Infrastructure.IGet<T, ID>")
            {
                implementsIGet = true;
                var entityTypeSymbol = baseInterface.TypeArguments[0];
                entityTypeName = entityTypeSymbol.Name;
                entityTypeNamespace = entityTypeSymbol.ContainingNamespace!.ToDisplayString();
                idTypeName = baseInterface.TypeArguments[1].ToDisplayString();
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IAdd<T>")
            {
                implementsIAdd = true;
                if (entityTypeName == null)
                {
                    var entityTypeSymbol = baseInterface.TypeArguments[0];
                    entityTypeName = entityTypeSymbol.Name;
                    entityTypeNamespace = entityTypeSymbol.ContainingNamespace!.ToDisplayString();
                }
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IUpdate<T, ID>")
            {
                implementsIUpdate = true;
                if (entityTypeName == null)
                {
                    var entityTypeSymbol = baseInterface.TypeArguments[0];
                    entityTypeName = entityTypeSymbol.Name;
                    entityTypeNamespace = entityTypeSymbol.ContainingNamespace!.ToDisplayString();
                }
                idTypeName ??= baseInterface.TypeArguments[1].ToDisplayString();
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IRemove<T, ID>")
            {
                implementsIRemove = true;
                if (entityTypeName == null)
                {
                    var entityTypeSymbol = baseInterface.TypeArguments[0];
                    entityTypeName = entityTypeSymbol.Name;
                    entityTypeNamespace = entityTypeSymbol.ContainingNamespace!.ToDisplayString();
                }
                idTypeName ??= baseInterface.TypeArguments[1].ToDisplayString();
            }
        }

        if (entityTypeName == null)
        {
            var generateRepoAttr = interfaceSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass!.Name == "GenerateRepositoryAttribute");

            if (generateRepoAttr?.AttributeClass!.TypeArguments.Length > 0)
            {
                var entityTypeSymbol = generateRepoAttr.AttributeClass!.TypeArguments[0];
                entityTypeName = entityTypeSymbol.Name;
                entityTypeNamespace = entityTypeSymbol.ContainingNamespace!.ToDisplayString();

                if (generateRepoAttr.AttributeClass!.TypeArguments.Length > 1)
                {
                    idTypeName = generateRepoAttr.AttributeClass!.TypeArguments[1].ToDisplayString();
                }
            }
        }

        if (entityTypeName == null)
        {
            var descriptor = new DiagnosticDescriptor(
                "FUDIE002",
                "No Entity Type Found",
                $"Interface {interfaceSymbol.Name} does not implement any Fudie repository interfaces (IGet, IAdd, IUpdate, IRemove) and does not have [GenerateRepository<TEntity>] attribute",
                "Fudie.Generator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, interfaceSymbol.Locations.FirstOrDefault()));
            return null;
        }

        idTypeName ??= "System.Guid";

        var entitySymbol = compilation.GetTypeByMetadataName($"{namespaceName}.{entityTypeName}");
        if (entitySymbol == null)
        {
            entitySymbol = FindEntityType(compilation, entityTypeName);
        }

        if (entitySymbol == null)
        {
            var descriptor = new DiagnosticDescriptor(
                "FUDIE003",
                "Entity Type Not Found",
                $"Could not find entity type '{entityTypeName}' in compilation",
                "Fudie.Generator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, interfaceSymbol.Locations.FirstOrDefault()));
            return null;
        }

        var includePaths = ExtractIncludePaths(interfaceSymbol, entitySymbol, compilation, context);

        var hasTrackingAttribute = HasAttribute(interfaceSymbol, "Fudie.Infrastructure.TrackingAttribute");
        var hasAsNoTrackingAttribute = HasAttribute(interfaceSymbol, "Fudie.Infrastructure.AsNoTrackingAttribute");
        var trackingValue = hasTrackingAttribute ? GetTrackingAttributeValue(interfaceSymbol) : (bool?)null;

        var asNoTracking = hasAsNoTrackingAttribute || (hasTrackingAttribute && trackingValue == false);

        var interfaceDefaultTracking = hasTrackingAttribute && trackingValue != false && !hasAsNoTrackingAttribute;

        var asSplitQuery = HasAttribute(interfaceSymbol, "Fudie.Infrastructure.AsSplitQueryAttribute");
        var ignoreQueryFilters = HasAttribute(interfaceSymbol, "Fudie.Infrastructure.IgnoreQueryFiltersAttribute");

        var queryMethods = ExtractQueryMethods(interfaceSymbol, entitySymbol, context, interfaceDefaultTracking);

        var additionalUsings = new List<string>();
        if (entityTypeNamespace is not null &&
            entityTypeNamespace.Length > 0 &&
            entityTypeNamespace != namespaceName &&
            entityTypeNamespace != "global")
        {
            additionalUsings.Add(entityTypeNamespace);
        }

        var containerInterfaceName = interfaceSymbol.Name;
        string containerInterfaceFullName;

        if (containingType != null)
        {
            containerInterfaceFullName = $"{containingType.Name}.{containerInterfaceName}";
        }
        else
        {
            containerInterfaceFullName = containerInterfaceName;
        }

        var baseInterfaceNames = new List<string>();
        var hasUpdateOrRemove = implementsIUpdate || implementsIRemove;

        foreach (var baseInterface in baseInterfaces)
        {
            var interfaceFullName = baseInterface.ConstructedFrom.ToDisplayString();

            if (interfaceFullName == "Fudie.Infrastructure.IGet<T, ID>")
            {
                if (!hasUpdateOrRemove)
                {
                    baseInterfaceNames.Add($"IGet<{entityTypeName}, {idTypeName}>");
                }
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IAdd<T>")
            {
                baseInterfaceNames.Add($"IAdd<{entityTypeName}>");
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IUpdate<T, ID>")
            {
                baseInterfaceNames.Add($"IUpdate<{entityTypeName}, {idTypeName}>");
            }
            else if (interfaceFullName == "Fudie.Infrastructure.IRemove<T, ID>")
            {
                baseInterfaceNames.Add($"IRemove<{entityTypeName}, {idTypeName}>");
            }
        }

        var builderConfig = new CodeBuilder.RepositoryConfig
        {
            ImplementIGet = implementsIGet,
            ImplementIAdd = implementsIAdd,
            ImplementIUpdate = implementsIUpdate,
            ImplementIRemove = implementsIRemove,
            IncludePaths = includePaths,
            AsNoTracking = asNoTracking,
            AsSplitQuery = asSplitQuery,
            IgnoreQueryFilters = ignoreQueryFilters,
            QueryMethods = queryMethods,
            AdditionalUsings = additionalUsings,
            ContainerInterfaceName = containerInterfaceName,
            ContainerInterfaceFullName = containerInterfaceFullName,
            BaseInterfaceNames = baseInterfaceNames
        };

        return new RepositoryConfiguration(
            Namespace: namespaceName,
            ClassName: className,
            EntityTypeName: entityTypeName,
            IdTypeName: idTypeName,
            BuilderConfig: builderConfig
        );
    }

    /// <summary>
    /// Extracts and validates the include navigation paths declared on the interface via <c>[Include]</c> attributes.
    /// </summary>
    /// <param name="interfaceSymbol">The repository interface symbol.</param>
    /// <param name="entitySymbol">The entity type symbol used for path validation.</param>
    /// <param name="compilation">The current compilation.</param>
    /// <param name="context">The source production context used to report diagnostics.</param>
    /// <returns>A list of validated <see cref="PathValidator.IncludePathInfo"/> entries.</returns>
    private static List<PathValidator.IncludePathInfo> ExtractIncludePaths(
        INamedTypeSymbol interfaceSymbol,
        INamedTypeSymbol entitySymbol,
        Compilation compilation,
        SourceProductionContext context)
    {
        var includePaths = new List<PathValidator.IncludePathInfo>();

        var includeAttributes = interfaceSymbol.GetAttributes()
            .Where(attr => attr.AttributeClass!.Name == "IncludeAttribute")
            .ToList();

        foreach (var attr in includeAttributes)
        {
            if (attr.ConstructorArguments.Length > 0)
            {
                var pathsArgument = attr.ConstructorArguments[0];

                if (pathsArgument.Kind == TypedConstantKind.Array)
                {
                    foreach (var pathValue in pathsArgument.Values)
                    {
                        if (pathValue.Value is string path)
                        {
                            var pathInfo = PathValidator.ValidatePath(
                                path,
                                entitySymbol,
                                compilation,
                                attr.ApplicationSyntaxReference!.GetSyntax().GetLocation());

                            if (!pathInfo.IsValid)
                            {
                                var descriptor = new DiagnosticDescriptor(
                                    "FUDIE004",
                                    "Invalid Include Path",
                                    pathInfo.ErrorMessage ?? "Invalid include path",
                                    "Fudie.Generator",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true);

                                context.ReportDiagnostic(Diagnostic.Create(
                                    descriptor,
                                    pathInfo.Location ?? attr.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
                            }
                            else
                            {
                                includePaths.Add(pathInfo);
                            }
                        }
                    }
                }
            }
        }

        return includePaths;
    }

    /// <summary>
    /// Determines whether the given named type symbol has an attribute with the specified fully-qualified name.
    /// </summary>
    /// <param name="symbol">The type symbol to inspect.</param>
    /// <param name="fullAttributeName">The fully-qualified name of the attribute to look for.</param>
    /// <returns><see langword="true"/> if the attribute is present; otherwise, <see langword="false"/>.</returns>
    private static bool HasAttribute(INamedTypeSymbol symbol, string fullAttributeName)
    {
        return symbol.GetAttributes()
            .Any(attr => attr.AttributeClass!.ToDisplayString() == fullAttributeName);
    }

    /// <summary>
    /// Reads the boolean argument of a <c>[Tracking]</c> attribute applied to the given symbol.
    /// </summary>
    /// <param name="symbol">The type symbol that carries the attribute.</param>
    /// <returns>The explicit boolean value when provided; <see langword="null"/> when the attribute has no argument.</returns>
    private static bool? GetTrackingAttributeValue(INamedTypeSymbol symbol)
    {
        var trackingAttr = symbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.Name == "TrackingAttribute");

        if (trackingAttr?.ConstructorArguments.Length > 0)
        {
            var value = trackingAttr.ConstructorArguments[0].Value;
            if (value is bool boolValue)
            {
                return boolValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Searches all syntax trees in the compilation for a class declaration whose name matches the given entity type name.
    /// </summary>
    /// <param name="compilation">The current compilation to search.</param>
    /// <param name="entityTypeName">The simple (unqualified) name of the entity type to find.</param>
    /// <returns>The matching <see cref="INamedTypeSymbol"/> when found; otherwise, <see langword="null"/>.</returns>
    private static INamedTypeSymbol? FindEntityType(Compilation compilation, string entityTypeName)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            foreach (var node in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var symbol = semanticModel.GetDeclaredSymbol(node) as INamedTypeSymbol;
                if (symbol!.Name == entityTypeName)
                {
                    return symbol;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns all properties declared on the given type and its entire base-type chain.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to start from, or <see langword="null"/> to return an empty sequence.</param>
    /// <returns>All property symbols reachable through inheritance.</returns>
    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol? typeSymbol)
    {
        var properties = new List<IPropertySymbol>();
        var currentType = typeSymbol;

        while (currentType != null)
        {
            properties.AddRange(currentType.GetMembers().OfType<IPropertySymbol>());
            currentType = currentType.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Extracts and validates the query methods declared on the repository interface.
    /// </summary>
    /// <param name="interfaceSymbol">The repository interface symbol.</param>
    /// <param name="entitySymbol">The entity type symbol used for validation.</param>
    /// <param name="context">The source production context used to report diagnostics.</param>
    /// <param name="interfaceUseTracking">The tracking default inherited from the interface level, applied to methods that carry no explicit tracking attribute.</param>
    /// <returns>A list of valid <see cref="CodeBuilder.QueryMethodInfo"/> descriptors ready for code generation.</returns>
    private static List<CodeBuilder.QueryMethodInfo> ExtractQueryMethods(
        INamedTypeSymbol interfaceSymbol,
        INamedTypeSymbol entitySymbol,
        SourceProductionContext context,
        bool interfaceUseTracking)
    {
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>();
        var parser = new QueryMethod.QueryParser();
        var validator = new QueryMethod.QueryValidator();

        var entityProperties = GetAllProperties(entitySymbol)
            .Select(p => p.Name)
            .ToList();

        var methods = interfaceSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => !IsInfrastructureMethod(m))
            .ToList();

        foreach (var method in methods)
        {
            var parseResult = parser.Parse(method.Name, entityProperties);

            if (!parseResult.Success)
            {
                var diagnostic = QueryMethod.Diagnostics.CreateParseError(
                    method.Name,
                    parseResult.ErrorMessage ?? "Unknown error",
                    method.Locations.FirstOrDefault());
                context.ReportDiagnostic(diagnostic);
                continue;
            }

            var validationDiagnostics = validator.Validate(
                parseResult.Query!,
                method,
                entitySymbol,
                method.Locations.FirstOrDefault());

            foreach (var diagnostic in validationDiagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            if (validationDiagnostics.Any())
            {
                continue;
            }

            var parameters = method.Parameters
                .Select(p => (p.Name, p.Type.ToDisplayString()))
                .ToList();

            var methodUseTracking = GetMethodUseTracking(method, interfaceUseTracking);

            queryMethods.Add(new CodeBuilder.QueryMethodInfo
            {
                MethodName = method.Name,
                ParseResult = parseResult,
                Parameters = parameters,
                UseTracking = methodUseTracking
            });
        }

        return queryMethods;
    }

    /// <summary>
    /// Determines the effective <c>UseTracking</c> value for a specific query method.
    /// Method-level attributes take priority over the interface-level default.
    /// </summary>
    /// <param name="method">The method symbol to inspect.</param>
    /// <param name="interfaceDefault">The tracking default inherited from the interface.</param>
    /// <returns><see langword="true"/> if the method should use change tracking; otherwise, <see langword="false"/>.</returns>
    private static bool GetMethodUseTracking(IMethodSymbol method, bool interfaceDefault)
    {
        var hasAsNoTracking = method.GetAttributes()
            .Any(attr => attr.AttributeClass!.Name == "AsNoTrackingAttribute");

        if (hasAsNoTracking)
            return false;

        var trackingAttr = method.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass!.Name == "TrackingAttribute");

        if (trackingAttr != null)
        {
            if (trackingAttr.ConstructorArguments.Length > 0 &&
                trackingAttr.ConstructorArguments[0].Value is bool trackingValue)
            {
                return trackingValue;
            }
            return true;
        }

        return interfaceDefault;
    }

    /// <summary>
    /// Determines whether the given method is defined on one of the Fudie infrastructure interfaces
    /// (<c>IGet</c>, <c>IAdd</c>, <c>IUpdate</c>, or <c>IRemove</c>).
    /// </summary>
    /// <param name="method">The method symbol to evaluate.</param>
    /// <returns><see langword="true"/> if the method belongs to a Fudie infrastructure interface; otherwise, <see langword="false"/>.</returns>
    private static bool IsInfrastructureMethod(IMethodSymbol method)
    {
        var containingType = method.ContainingType;
        if (containingType == null)
            return false;

        var fullName = containingType.ConstructedFrom.ToDisplayString();

        return fullName == "Fudie.Infrastructure.IGet<T, ID>" ||
               fullName == "Fudie.Infrastructure.IAdd<T>" ||
               fullName == "Fudie.Infrastructure.IUpdate<T, ID>" ||
               fullName == "Fudie.Infrastructure.IRemove<T, ID>";
    }

    /// <summary>
    /// Holds the syntax node and semantic symbol for a discovered repository interface.
    /// </summary>
    private record RepositoryInterfaceInfo(
        InterfaceDeclarationSyntax Syntax,
        INamedTypeSymbol Symbol
    );

    /// <summary>
    /// Aggregates all data needed to generate a single repository class.
    /// </summary>
    private record RepositoryConfiguration(
        string Namespace,
        string ClassName,
        string EntityTypeName,
        string IdTypeName,
        CodeBuilder.RepositoryConfig BuilderConfig
    );

}

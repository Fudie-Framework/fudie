namespace Fudie.Generator;

/// <summary>
/// Generates C# code for repository implementations.
/// </summary>
internal static class CodeBuilder
{
    /// <summary>
    /// Generates the complete Include/ThenInclude chain code for a validated path.
    /// </summary>
    /// <param name="pathInfo">Validated path information.</param>
    /// <param name="entityTypeName">Name of the root entity type.</param>
    /// <param name="queryVariableName">Name of the query variable (e.g. "query").</param>
    /// <returns>Include/ThenInclude chain code.</returns>
    public static string GenerateIncludeChain(
        PathValidator.IncludePathInfo pathInfo,
        string entityTypeName,
        string queryVariableName = "query")
    {
        if (!pathInfo.IsValid || pathInfo.SegmentDetails.Length == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var parameterNames = new List<string>();

        var rootParameter = GenerateParameterName(entityTypeName);
        parameterNames.Add(rootParameter);

        var firstSegment = pathInfo.SegmentDetails[0];
        sb.Append($"{queryVariableName} = {queryVariableName}.Include({rootParameter} => {rootParameter}.{firstSegment.PropertyName})");

        for (int i = 1; i < pathInfo.SegmentDetails.Length; i++)
        {
            var segment = pathInfo.SegmentDetails[i];
            var previousSegment = pathInfo.SegmentDetails[i - 1];

            string currentParameter;

            if (previousSegment.IsCollection)
            {
                currentParameter = GenerateParameterName(previousSegment.ElementType!.Name);
            }
            else
            {
                currentParameter = GenerateParameterName(previousSegment.PropertyType.Name);
            }

            parameterNames.Add(currentParameter);

            sb.AppendLine()
              .Append($"    .ThenInclude({currentParameter} => {currentParameter}.{segment.PropertyName})");
        }

        sb.Append(";");

        return sb.ToString();
    }

    /// <summary>
    /// Generates multiple Include chains for several paths.
    /// </summary>
    /// <param name="paths">List of validated paths.</param>
    /// <param name="entityTypeName">Name of the entity type.</param>
    /// <param name="queryVariableName">Name of the query variable.</param>
    /// <returns>Code with all Include statements.</returns>
    public static string GenerateMultipleIncludes(
        IEnumerable<PathValidator.IncludePathInfo> paths,
        string entityTypeName,
        string queryVariableName = "query")
    {
        var sb = new StringBuilder();
        var validPaths = paths.Where(p => p.IsValid).ToArray();

        for (int i = 0; i < validPaths.Length; i++)
        {
            var includeCode = GenerateIncludeChain(validPaths[i], entityTypeName, queryVariableName);
            sb.Append(includeCode);

            if (i < validPaths.Length - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates code to apply AsNoTracking.
    /// </summary>
    public static string GenerateAsNoTracking(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.AsNoTracking();";
    }

    /// <summary>
    /// Generates code to apply AsSplitQuery.
    /// </summary>
    public static string GenerateAsSplitQuery(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.AsSplitQuery();";
    }

    /// <summary>
    /// Generates code to apply IgnoreQueryFilters.
    /// </summary>
    public static string GenerateIgnoreQueryFilters(string queryVariableName = "query")
    {
        return $"{queryVariableName} = {queryVariableName}.IgnoreQueryFilters();";
    }

    /// <summary>
    /// Generates the complete Get method for IGet&lt;T, ID&gt;.
    /// </summary>
    public static string GenerateGetMethod(
        string entityTypeName,
        string idTypeName,
        IEnumerable<PathValidator.IncludePathInfo> includePaths,
        bool asNoTracking,
        bool asSplitQuery,
        bool ignoreQueryFilters)
    {
        var sb = new StringBuilder();
        var entityParameter = GenerateParameterName(entityTypeName);

        sb.AppendLine($"public async Task<{entityTypeName}> Get({idTypeName} id)");
        sb.AppendLine("{");
        sb.AppendLine($"    IQueryable<{entityTypeName}> query = _entityLookup.Set<{entityTypeName}>();");
        sb.AppendLine();

        var validPaths = includePaths.Where(p => p.IsValid).ToArray();
        if (validPaths.Any())
        {
            sb.AppendLine("    // Apply includes");
            foreach (var path in validPaths)
            {
                var includeCode = GenerateIncludeChain(path, entityTypeName, "query");
                sb.AppendLine($"    {includeCode}");
            }
            sb.AppendLine();
        }

        var hasModifiers = asNoTracking || asSplitQuery || ignoreQueryFilters;
        if (hasModifiers)
        {
            sb.AppendLine("    // Apply query modifiers");
            if (asSplitQuery)
            {
                sb.AppendLine($"    {GenerateAsSplitQuery("query")}");
            }
            if (asNoTracking)
            {
                sb.AppendLine($"    {GenerateAsNoTracking("query")}");
            }
            if (ignoreQueryFilters)
            {
                sb.AppendLine($"    {GenerateIgnoreQueryFilters("query")}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"    var entity = await query.FirstOrDefaultAsync({entityParameter} => {entityParameter}.Id == id);");
        sb.AppendLine();
        sb.AppendLine("    if (entity == null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        throw new KeyNotFoundException($\"{entityTypeName} with ID '{{id}}' not found.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    return entity;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the complete Add method for IAdd&lt;T&gt;.
    /// </summary>
    public static string GenerateAddMethod(string entityTypeName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"public void Add({entityTypeName} entity)");
        sb.AppendLine("{");
        sb.AppendLine("    _changeTracker.Entry(entity).State = EntityState.Added;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the complete Remove method for IRemove&lt;T, ID&gt;.
    /// </summary>
    public static string GenerateRemoveMethod(string entityTypeName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"public void Remove({entityTypeName} entity)");
        sb.AppendLine("{");
        sb.AppendLine("    _changeTracker.Entry(entity).State = EntityState.Deleted;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the complete Get method for IUpdate&lt;T, ID&gt;.
    /// Same as IGet but with tracking enabled (uses Set instead of Query).
    /// </summary>
    public static string GenerateUpdateGetMethod(
        string entityTypeName,
        string idTypeName,
        IEnumerable<PathValidator.IncludePathInfo> includePaths,
        bool asSplitQuery,
        bool ignoreQueryFilters = false)
    {
        var sb = new StringBuilder();
        var entityParameter = GenerateParameterName(entityTypeName);

        sb.AppendLine($"public async Task<{entityTypeName}> Get({idTypeName} id)");
        sb.AppendLine("{");
        sb.AppendLine($"    IQueryable<{entityTypeName}> query = _entityLookup.Set<{entityTypeName}>();");
        sb.AppendLine();

        var validPaths = includePaths.Where(p => p.IsValid).ToArray();
        if (validPaths.Any())
        {
            sb.AppendLine("    // Apply includes");
            foreach (var path in validPaths)
            {
                var includeCode = GenerateIncludeChain(path, entityTypeName, "query");
                sb.AppendLine($"    {includeCode}");
            }
            sb.AppendLine();
        }

        var hasModifiers = asSplitQuery || ignoreQueryFilters;
        if (hasModifiers)
        {
            sb.AppendLine("    // Apply query modifiers");
            if (asSplitQuery)
            {
                sb.AppendLine($"    {GenerateAsSplitQuery("query")}");
            }
            if (ignoreQueryFilters)
            {
                sb.AppendLine($"    {GenerateIgnoreQueryFilters("query")}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"    var entity = await query.FirstOrDefaultAsync({entityParameter} => {entityParameter}.Id == id);");
        sb.AppendLine();
        sb.AppendLine("    if (entity == null)");
        sb.AppendLine("    {");
        sb.AppendLine($"        throw new KeyNotFoundException($\"{entityTypeName} with ID '{{id}}' not found.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    return entity;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a complete repository class.
    /// </summary>
    public static string GenerateRepositoryClass(
    string className,
    string namespaceName,
    string entityTypeName,
    string idTypeName,
    RepositoryConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Fudie.Infrastructure;");
        sb.AppendLine("using Fudie.DependencyInjection;");

        foreach (var additionalUsing in config.AdditionalUsings)
        {
            sb.AppendLine($"using {additionalUsing};");
        }

        sb.AppendLine();

        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        var baseInterfaces = new List<string>();
        if (config.ImplementIGet)
            baseInterfaces.Add($"IGet<{entityTypeName}, {idTypeName}>");
        if (config.ImplementIAdd)
            baseInterfaces.Add($"IAdd<{entityTypeName}>");
        if (config.ImplementIUpdate)
            baseInterfaces.Add($"IUpdate<{entityTypeName}, {idTypeName}>");
        if (config.ImplementIRemove)
            baseInterfaces.Add($"IRemove<{entityTypeName}, {idTypeName}>");

        sb.AppendLine("[ExcludeFromCodeCoverage]");
        sb.AppendLine("[DebuggerNonUserCode]");
        sb.AppendLine("[EditorBrowsable(EditorBrowsableState.Never)]");

        if (!string.IsNullOrEmpty(config.ContainerInterfaceFullName))
        {
            sb.AppendLine($"[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof({config.ContainerInterfaceFullName}))]");
        }
        else
        {
            sb.AppendLine("[Injectable(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped)]");
        }

        sb.Append($"public class {className}");

        if (!string.IsNullOrEmpty(config.ContainerInterfaceFullName))
        {
            sb.Append($" : {config.ContainerInterfaceFullName}");
        }
        else if (baseInterfaces.Any())
        {
            sb.Append($" : {string.Join(", ", baseInterfaces)}");
        }

        sb.AppendLine();
        sb.AppendLine("{");

        var fields = new List<string>();

        var anyMethodUsesTracking = config.QueryMethods.Any(m => m.UseTracking);
        var needsEntityLookup = config.ImplementIGet || config.ImplementIUpdate || config.ImplementIRemove || anyMethodUsesTracking;

        var anyMethodNoTracking = config.QueryMethods.Any(m => !m.UseTracking);
        var needsQuery = anyMethodNoTracking;

        if (needsEntityLookup)
        {
            fields.Add("private readonly IEntityLookup _entityLookup;");
        }
        if (config.ImplementIAdd || config.ImplementIRemove)
        {
            fields.Add("private readonly IChangeTracker _changeTracker;");
        }
        if (needsQuery)
        {
            fields.Add("private readonly IQuery _query;");
        }

        foreach (var field in fields)
        {
            sb.AppendLine($"    {field}");
        }

        if (fields.Any())
        {
            sb.AppendLine();
        }

        var constructorParams = new List<string>();
        if (fields.Contains("private readonly IEntityLookup _entityLookup;"))
            constructorParams.Add("IEntityLookup entityLookup");
        if (fields.Contains("private readonly IChangeTracker _changeTracker;"))
            constructorParams.Add("IChangeTracker changeTracker");
        if (fields.Contains("private readonly IQuery _query;"))
            constructorParams.Add("IQuery query");

        sb.AppendLine($"    public {className}({string.Join(", ", constructorParams)})");
        sb.AppendLine("    {");
        if (constructorParams.Contains("IEntityLookup entityLookup"))
            sb.AppendLine("        _entityLookup = entityLookup;");
        if (constructorParams.Contains("IChangeTracker changeTracker"))
            sb.AppendLine("        _changeTracker = changeTracker;");
        if (constructorParams.Contains("IQuery query"))
            sb.AppendLine("        _query = query;");
        sb.AppendLine("    }");
        sb.AppendLine();

        if (config.ImplementIUpdate || config.ImplementIRemove)
        {
            var updateGetMethod = GenerateUpdateGetMethod(
                entityTypeName,
                idTypeName,
                config.IncludePaths,
                config.AsSplitQuery,
                config.IgnoreQueryFilters);

            var lines = updateGetMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }
        else if (config.ImplementIGet)
        {
            var getMethod = GenerateGetMethod(
                entityTypeName,
                idTypeName,
                config.IncludePaths,
                config.AsNoTracking,
                config.AsSplitQuery,
                config.IgnoreQueryFilters);

            var lines = getMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }

        if (config.ImplementIAdd)
        {
            var addMethod = GenerateAddMethod(entityTypeName);
            var lines = addMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
            sb.AppendLine();
        }

        if (config.ImplementIRemove)
        {
            var removeMethod = GenerateRemoveMethod(entityTypeName);
            var lines = removeMethod.Split('\n');
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine($"    {line.TrimEnd()}");
                }
            }
        }

        if (config.QueryMethods.Any())
        {
            var queryMethodsCode = GenerateQueryMethods(config.QueryMethods, entityTypeName, config);
            if (!string.IsNullOrWhiteSpace(queryMethodsCode))
            {
                sb.Append(queryMethodsCode);
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a lambda parameter name based on the type name.
    /// </summary>
    /// <param name="typeName">Type name (e.g. "Customer", "Order", "OrderItem").</param>
    /// <returns>Parameter name (e.g. "c", "o", "oi").</returns>
    private static string GenerateParameterName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return "x";

        bool isCompoundName = typeName.Length > 1 &&
                             typeName.Skip(1).Any(c => char.IsUpper(c));

        if (isCompoundName)
        {
            var initials = new List<char>();

            initials.Add(char.ToLowerInvariant(typeName[0]));

            for (int i = 1; i < typeName.Length; i++)
            {
                if (char.IsUpper(typeName[i]))
                {
                    initials.Add(char.ToLowerInvariant(typeName[i]));
                }
            }

            return string.Join("", initials);
        }

        return char.ToLowerInvariant(typeName[0]).ToString();
    }


    /// <summary>
    /// Generates query methods from a list of method descriptors.
    /// </summary>
    /// <param name="queryMethods">List of query methods to generate.</param>
    /// <param name="entityTypeName">Name of the entity type.</param>
    /// <param name="config">Repository configuration with interface modifiers.</param>
    public static string GenerateQueryMethods(
        IEnumerable<QueryMethodInfo> queryMethods,
        string entityTypeName,
        RepositoryConfig? config = null)
    {
        if (queryMethods == null || !queryMethods.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var emitter = new QueryMethod.LinqEmitter();

        var includeChains = BuildIncludeChains(config?.IncludePaths, entityTypeName);

        foreach (var queryMethod in queryMethods)
        {
            if (!queryMethod.ParseResult.Success || queryMethod.ParseResult.Query == null)
                continue;

            var query = queryMethod.ParseResult.Query;
            var methodName = queryMethod.MethodName;
            var parameters = queryMethod.Parameters.ToArray();

            var signature = emitter.EmitMethodSignature(
                query,
                methodName,
                entityTypeName,
                parameters);

            sb.AppendLine($"    {signature}");
            sb.AppendLine("    {");

            var paramNames = parameters.Select(p => p.name).ToArray();
            var queryCode = emitter.Emit(
                query, methodName, entityTypeName, paramNames,
                queryMethod.UseTracking,
                config?.IgnoreQueryFilters ?? false,
                config?.AsSplitQuery ?? false,
                includeChains);

            sb.AppendLine($"        return await {queryCode};");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds fluent-style Include/ThenInclude chain strings from validated paths.
    /// </summary>
    private static List<string>? BuildIncludeChains(
        IEnumerable<PathValidator.IncludePathInfo>? paths,
        string entityTypeName)
    {
        if (paths == null)
            return null;

        var validPaths = paths.Where(p => p.IsValid).ToArray();
        if (validPaths.Length == 0)
            return null;

        var chains = new List<string>();

        foreach (var pathInfo in validPaths)
        {
            if (pathInfo.SegmentDetails.Length == 0)
                continue;

            var rootParam = GenerateParameterName(entityTypeName);
            var first = pathInfo.SegmentDetails[0];
            var chain = new StringBuilder();
            chain.Append($".Include({rootParam} => {rootParam}.{first.PropertyName})");

            for (int i = 1; i < pathInfo.SegmentDetails.Length; i++)
            {
                var segment = pathInfo.SegmentDetails[i];
                var prev = pathInfo.SegmentDetails[i - 1];
                var param = prev.IsCollection
                    ? GenerateParameterName(prev.ElementType!.Name)
                    : GenerateParameterName(prev.PropertyType.Name);

                chain.AppendLine();
                chain.Append($"                .ThenInclude({param} => {param}.{segment.PropertyName})");
            }

            chains.Add(chain.ToString());
        }

        return chains.Count > 0 ? chains : null;
    }

    /// <summary>
    /// Describes a query method to be generated for a repository.
    /// </summary>
    public class QueryMethodInfo
    {
        /// <summary>
        /// Name of the method.
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// Parse result containing the structured query representation.
        /// </summary>
        public QueryMethod.ParseResult ParseResult { get; set; } = QueryMethod.ParseResult.Error("Not parsed");

        /// <summary>
        /// Ordered list of parameter name/type pairs for the generated method signature.
        /// </summary>
        public List<(string name, string type)> Parameters { get; set; } = new();

        /// <summary>
        /// Indicates whether this specific method uses change-tracking.
        /// Determined by: method attribute > interface attribute > default (false).
        /// </summary>
        public bool UseTracking { get; set; }
    }

    /// <summary>
    /// Configuration for repository code generation.
    /// </summary>
    public class RepositoryConfig
    {
        /// <summary>
        /// Whether the repository should implement IGet&lt;T, ID&gt;.
        /// </summary>
        public bool ImplementIGet { get; set; }

        /// <summary>
        /// Whether the repository should implement IAdd&lt;T&gt;.
        /// </summary>
        public bool ImplementIAdd { get; set; }

        /// <summary>
        /// Whether the repository should implement IUpdate&lt;T, ID&gt;.
        /// </summary>
        public bool ImplementIUpdate { get; set; }

        /// <summary>
        /// Whether the repository should implement IRemove&lt;T, ID&gt;.
        /// </summary>
        public bool ImplementIRemove { get; set; }

        /// <summary>
        /// Validated include paths to apply in generated Get methods.
        /// </summary>
        public IEnumerable<PathValidator.IncludePathInfo> IncludePaths { get; set; } = Array.Empty<PathValidator.IncludePathInfo>();

        /// <summary>
        /// Whether to apply AsNoTracking on queries.
        /// </summary>
        public bool AsNoTracking { get; set; }

        /// <summary>
        /// Whether to apply AsSplitQuery on queries.
        /// </summary>
        public bool AsSplitQuery { get; set; }

        /// <summary>
        /// Whether to apply IgnoreQueryFilters on queries.
        /// </summary>
        public bool IgnoreQueryFilters { get; set; }

        /// <summary>
        /// Indicates whether query methods should use tracking (IEntityLookup.Set) instead of IQuery.Query.
        /// </summary>
        public bool QueryMethodsUseTracking { get; set; }

        /// <summary>
        /// List of query methods to generate on the repository class.
        /// </summary>
        public List<QueryMethodInfo> QueryMethods { get; set; } = new();

        /// <summary>
        /// Additional using directives to include in the generated file (e.g. entity type namespaces).
        /// </summary>
        public IEnumerable<string> AdditionalUsings { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Name of the container interface to implement (e.g. "IRepository", "ICustomerRepository").
        /// </summary>
        public string? ContainerInterfaceName { get; set; }

        /// <summary>
        /// Fully-qualified name of the container interface for typeof() (e.g. "CreateIngredient.IRepository").
        /// </summary>
        public string? ContainerInterfaceFullName { get; set; }

        /// <summary>
        /// List of base interface names with their full names for additional [Injectable] attributes
        /// (e.g. "IAdd&lt;Ingredient&gt;", "IGet&lt;Ingredient, Guid&gt;").
        /// </summary>
        public List<string> BaseInterfaceNames { get; set; } = new();
    }
}

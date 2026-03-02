namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Generates LINQ code for parsed queries.
/// </summary>
public class LinqEmitter
{
    /// <summary>
    /// Generates the complete method body code for a parsed query.
    /// </summary>
    /// <param name="query">The parsed query.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="parameters">The parameter names to use in the generated expression.</param>
    /// <param name="useTracking">When <c>true</c>, uses <c>_entityLookup.Set</c> (with tracking); otherwise uses <c>_query.Query</c> (without tracking).</param>
    /// <param name="ignoreQueryFilters">When <c>true</c>, appends <c>.IgnoreQueryFilters()</c> to the query.</param>
    /// <param name="asSplitQuery">When <c>true</c>, appends <c>.AsSplitQuery()</c> to the query.</param>
    /// <param name="includeChains">Pre-formatted Include/ThenInclude chains to insert into the fluent chain.</param>
    /// <returns>The generated C# query expression.</returns>
    public string Emit(ParsedQuery query, string methodName, string entityName, string[] parameters,
        bool useTracking = false, bool ignoreQueryFilters = false, bool asSplitQuery = false,
        IEnumerable<string>? includeChains = null)
    {
        var sb = new StringBuilder();

        var querySource = useTracking
            ? $"_entityLookup.Set<{entityName}>()"
            : $"_query.Query<{entityName}>()";
        sb.Append(querySource);

        if (ignoreQueryFilters)
        {
            sb.AppendLine();
            sb.Append("            .IgnoreQueryFilters()");
        }

        if (includeChains != null)
        {
            foreach (var chain in includeChains)
            {
                sb.AppendLine();
                sb.Append($"            {chain}");
            }
        }

        if (asSplitQuery)
        {
            sb.AppendLine();
            sb.Append("            .AsSplitQuery()");
        }

        if (query.Conditions.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"            .Where(x => {GenerateWhereExpression(query.Conditions, parameters)})");
        }

        if (query.OrderBy.Count > 0)
        {
            sb.AppendLine();
            sb.Append($"            {GenerateOrderByExpression(query.OrderBy)}");
        }

        if (query.Top.HasValue)
        {
            sb.AppendLine();
            sb.Append($"            .Take({query.Top.Value})");
        }

        sb.AppendLine();
        sb.Append($"            {GenerateFinalExecution(query)}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates the method signature for a parsed query method.
    /// </summary>
    /// <param name="query">The parsed query.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="parameters">The parameters with their names and types.</param>
    /// <returns>The generated method signature string.</returns>
    public string EmitMethodSignature(
        ParsedQuery query,
        string methodName,
        string entityName,
        (string name, string type)[] parameters)
    {
        var returnType = GetReturnType(query, entityName);
        var paramList = string.Join(", ", parameters.Select(p => $"{p.type} {p.name}"));

        return $"public async {returnType} {methodName}({paramList})";
    }

    /// <summary>
    /// Returns the async return type for the given query type and entity.
    /// </summary>
    /// <param name="query">The parsed query.</param>
    /// <param name="entityName">The name of the entity.</param>
    /// <returns>The C# return type string.</returns>
    private string GetReturnType(ParsedQuery query, string entityName)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => $"Task<{entityName}?>",
            QueryType.Find => $"Task<List<{entityName}>>",
            QueryType.Count => "Task<int>",
            QueryType.Exists => "Task<bool>",
            QueryType.Delete => "Task<int>",
            _ => "Task"
        };
    }

    /// <summary>
    /// Generates the complete WHERE lambda expression for a list of conditions.
    /// </summary>
    /// <param name="conditions">The conditions to combine.</param>
    /// <param name="parameters">The parameter names to bind to each condition.</param>
    /// <returns>The generated WHERE expression string.</returns>
    private string GenerateWhereExpression(List<Condition> conditions, string[] parameters)
    {
        var sb = new StringBuilder();
        var paramIndex = 0;

        for (int i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];

            if (i > 0)
            {
                sb.Append(condition.Or ? " || " : " && ");
            }

            var conditionExpr = GenerateConditionExpression(condition, parameters, ref paramIndex);
            sb.Append(conditionExpr);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates the expression for a single condition.
    /// </summary>
    /// <param name="condition">The condition to generate code for.</param>
    /// <param name="parameters">The available parameter names.</param>
    /// <param name="paramIndex">The current parameter index, incremented as parameters are consumed.</param>
    /// <returns>The generated condition expression string.</returns>
    private string GenerateConditionExpression(Condition condition, string[] parameters, ref int paramIndex)
    {
        var property = $"x.{condition.Property}";

        if (condition.IgnoreCase)
        {
            property = $"{property}.ToLower()";
        }

        switch (condition.Op)
        {
            case Operator.Equal:
                var value = condition.IgnoreCase ? $"{parameters[paramIndex]}.ToLower()" : parameters[paramIndex];
                paramIndex++;
                return $"{property} == {value}";

            case Operator.NotEqual:
                value = condition.IgnoreCase ? $"{parameters[paramIndex]}.ToLower()" : parameters[paramIndex];
                paramIndex++;
                return $"{property} != {value}";

            case Operator.LessThan:
                paramIndex++;
                return $"{property} < {parameters[paramIndex - 1]}";

            case Operator.LessThanOrEqual:
                paramIndex++;
                return $"{property} <= {parameters[paramIndex - 1]}";

            case Operator.GreaterThan:
                paramIndex++;
                return $"{property} > {parameters[paramIndex - 1]}";

            case Operator.GreaterThanOrEqual:
                paramIndex++;
                return $"{property} >= {parameters[paramIndex - 1]}";

            case Operator.Between:
                var min = parameters[paramIndex++];
                var max = parameters[paramIndex++];
                return $"{property} >= {min} && {property} <= {max}";

            case Operator.In:
                var collection = parameters[paramIndex++];
                return $"{collection}.Contains({property})";

            case Operator.NotIn:
                collection = parameters[paramIndex++];
                return $"!{collection}.Contains({property})";

            case Operator.StartsWith:
                value = parameters[paramIndex++];
                return $"{property}.StartsWith({value})";

            case Operator.EndsWith:
                value = parameters[paramIndex++];
                return $"{property}.EndsWith({value})";

            case Operator.Contains:
                value = parameters[paramIndex++];
                return $"{property}.Contains({value})";

            case Operator.Like:
                value = parameters[paramIndex++];
                return $"EF.Functions.Like({property}, {value})";

            case Operator.IsNull:
                return $"{property} == null";

            case Operator.IsNotNull:
                return $"{property} != null";

            case Operator.True:
                return $"{property} == true";

            case Operator.False:
                return $"{property} == false";

            default:
                throw new InvalidOperationException($"Unsupported operator: {condition.Op}");
        }
    }

    /// <summary>
    /// Generates the OrderBy fluent call for the given list of ordering specifications.
    /// </summary>
    /// <param name="orderByList">The list of OrderBy specifications.</param>
    /// <returns>The generated OrderBy expression string, or an empty string if the list is empty.</returns>
    internal string GenerateOrderByExpression(List<OrderBy> orderByList)
    {
        if (orderByList.Count == 0)
            return string.Empty;

        var first = orderByList[0];
        var method = first.Descending ? "OrderByDescending" : "OrderBy";

        return $".{method}(x => x.{first.Property})";
    }

    /// <summary>
    /// Generates the terminal async execution call based on the query type.
    /// </summary>
    /// <param name="query">The parsed query.</param>
    /// <returns>The generated terminal call string.</returns>
    private string GenerateFinalExecution(ParsedQuery query)
    {
        return query.Type switch
        {
            QueryType.Find when query.First || query.Top == 1 => ".FirstOrDefaultAsync()",
            QueryType.Find => ".ToListAsync()",
            QueryType.Count => ".CountAsync()",
            QueryType.Exists => ".AnyAsync()",
            QueryType.Delete => ".ExecuteDeleteAsync()",
            _ => throw new InvalidOperationException($"Unsupported query type: {query.Type}")
        };
    }

    /// <summary>
    /// Generates a complete method with its signature and body.
    /// </summary>
    /// <param name="query">The parsed query.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="entityName">The name of the entity.</param>
    /// <param name="parameters">The parameters with their names and types.</param>
    /// <returns>The complete generated method source code.</returns>
    public string EmitFullMethod(
        ParsedQuery query,
        string methodName,
        string entityName,
        (string name, string type)[] parameters)
    {
        var sb = new StringBuilder();

        var signature = EmitMethodSignature(query, methodName, entityName, parameters);
        sb.AppendLine($"    {signature}");
        sb.AppendLine("    {");

        var paramNames = parameters.Select(p => p.name).ToArray();
        var queryCode = Emit(query, methodName, entityName, paramNames);

        sb.AppendLine($"        return await {queryCode};");
        sb.AppendLine("    }");

        return sb.ToString();
    }
}

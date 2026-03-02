namespace Fudie.Generator.QueryMethod;

/// <summary>
/// Parser for query method names based on naming conventions.
/// </summary>
public class QueryParser
{
    private static readonly string[] Prefixes = new[]
    {
        "FindFirstBy",
        "FindTopBy",
        "FindBy",
        "CountBy",
        "ExistsBy",
        "DeleteBy"
    };

    private static readonly Dictionary<string, Operator> OperatorKeywords = new()
    {
        { "LessThanEqual", Operator.LessThanOrEqual },
        { "LessThan", Operator.LessThan },
        { "GreaterThanEqual", Operator.GreaterThanOrEqual },
        { "GreaterThan", Operator.GreaterThan },
        { "Between", Operator.Between },
        { "NotIn", Operator.NotIn },
        { "In", Operator.In },
        { "IsNotNull", Operator.IsNotNull },
        { "IsNull", Operator.IsNull },
        { "StartingWith", Operator.StartsWith },
        { "EndingWith", Operator.EndsWith },
        { "Containing", Operator.Contains },
        { "Like", Operator.Like },
        { "Not", Operator.NotEqual },
        { "True", Operator.True },
        { "False", Operator.False }
    };

    /// <summary>
    /// Parses a method name and returns the resulting parsed query.
    /// </summary>
    /// <param name="methodName">The method name to parse.</param>
    /// <param name="entityProperties">The properties available on the entity.</param>
    /// <returns>The parse result.</returns>
    public ParseResult Parse(string methodName, IEnumerable<string> entityProperties)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return ParseResult.Error("Method name cannot be empty");
        }

        var properties = entityProperties.ToList();
        var tokens = TokenizePascalCase(methodName);
        var position = 0;

        var prefixResult = DetectPrefix(tokens, ref position);
        if (!prefixResult.Success)
        {
            return prefixResult;
        }

        var query = prefixResult.Query!;

        if (position >= tokens.Count)
        {
            return ParseResult.Error("Method name must have conditions after prefix");
        }

        var conditionsResult = ParseConditions(tokens, ref position, properties);
        if (!conditionsResult.Success)
        {
            return conditionsResult;
        }

        query = query with { Conditions = conditionsResult.Query!.Conditions };

        if (position < tokens.Count && tokens[position] == "Order")
        {
            var orderByResult = ParseOrderBy(tokens, ref position, properties);
            if (!orderByResult.Success)
            {
                return orderByResult;
            }

            query = query with { OrderBy = orderByResult.Query!.OrderBy };
        }

        if (position < tokens.Count)
        {
            return ParseResult.Error($"Unexpected tokens after parsing: {string.Join("", tokens.Skip(position))}", position);
        }

        return ParseResult.Ok(query);
    }

    /// <summary>
    /// Tokenizes a PascalCase string, also splitting on digit boundaries.
    /// </summary>
    /// <param name="input">The input string to tokenize.</param>
    /// <returns>A list of tokens extracted from the input.</returns>
    private List<string> TokenizePascalCase(string input)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        var lastWasDigit = false;

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            var isDigit = char.IsDigit(c);
            var isUpper = char.IsUpper(c);

            if (currentToken.Length > 0 && (isUpper || isDigit != lastWasDigit))
            {
                tokens.Add(currentToken.ToString());
                currentToken.Clear();
            }

            currentToken.Append(c);
            lastWasDigit = isDigit;
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }

    /// <summary>
    /// Detects and consumes the method prefix from the token list.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated on success.</param>
    /// <returns>A parse result containing the query skeleton for the detected prefix.</returns>
    private ParseResult DetectPrefix(List<string> tokens, ref int position)
    {
        var methodStart = string.Join("", tokens);

        if (methodStart.StartsWith("FindFirstBy"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "First", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find,
                First = true
            });
        }

        if (methodStart.StartsWith("FindTop"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "Top");

            if (position >= tokens.Count || !int.TryParse(tokens[position], out var topN))
            {
                return ParseResult.Error("Expected number after 'FindTop'", position);
            }

            position++;

            if (position >= tokens.Count || tokens[position] != "By")
            {
                return ParseResult.Error("Expected 'By' after 'FindTop{N}'", position);
            }

            position++;

            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find,
                Top = topN
            });
        }

        if (methodStart.StartsWith("FindBy"))
        {
            position = ConsumeTokens(tokens, 0, "Find", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Find
            });
        }

        if (methodStart.StartsWith("CountBy"))
        {
            position = ConsumeTokens(tokens, 0, "Count", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Count
            });
        }

        if (methodStart.StartsWith("ExistsBy"))
        {
            position = ConsumeTokens(tokens, 0, "Exists", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Exists
            });
        }

        if (methodStart.StartsWith("DeleteBy"))
        {
            position = ConsumeTokens(tokens, 0, "Delete", "By");
            return ParseResult.Ok(new ParsedQuery
            {
                Type = QueryType.Delete
            });
        }

        return ParseResult.Error($"Unknown method prefix. Expected one of: {string.Join(", ", Prefixes)}", 0);
    }

    /// <summary>
    /// Parses all conditions in the method name until an OrderBy clause or the end of tokens is reached.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated as tokens are consumed.</param>
    /// <param name="properties">The entity properties available for matching.</param>
    /// <returns>A parse result containing the list of parsed conditions.</returns>
    private ParseResult ParseConditions(List<string> tokens, ref int position, List<string> properties)
    {
        var conditions = new List<Condition>();
        var isOr = false;

        while (position < tokens.Count)
        {
            if (tokens[position] == "Order")
            {
                break;
            }

            var conditionResult = ParseSingleCondition(tokens, ref position, properties, isOr);
            if (!conditionResult.Success)
            {
                return conditionResult;
            }

            conditions.Add(conditionResult.Query!.Conditions[0]);

            if (position < tokens.Count)
            {
                if (tokens[position] == "And")
                {
                    position++;
                    isOr = false;
                }
                else if (tokens[position] == "Or")
                {
                    position++;
                    isOr = true;
                }
                else if (tokens[position] == "Order")
                {
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        if (conditions.Count == 0)
        {
            return ParseResult.Error("Expected at least one condition", position);
        }

        return ParseResult.Ok(new ParsedQuery
        {
            Conditions = conditions
        });
    }

    /// <summary>
    /// Parses a single condition from the token list, including property, operator, and optional IgnoreCase modifier.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated as tokens are consumed.</param>
    /// <param name="properties">The entity properties available for matching.</param>
    /// <param name="isOr">Whether this condition is joined to the previous one with OR.</param>
    /// <returns>A parse result containing the single parsed condition.</returns>
    private ParseResult ParseSingleCondition(List<string> tokens, ref int position, List<string> properties, bool isOr)
    {
        var startPosition = position;

        var propertyResult = FindProperty(tokens, ref position, properties);
        if (propertyResult == null)
        {
            return ParseResult.Error($"Could not find valid property starting at position {startPosition}", startPosition);
        }

        var propertyName = propertyResult;
        var op = Operator.Equal;
        var ignoreCase = false;

        if (position < tokens.Count)
        {
            var operatorResult = TryParseOperator(tokens, ref position);
            if (operatorResult.HasValue)
            {
                op = operatorResult.Value;
            }
        }

        if (position < tokens.Count && tokens[position] == "Ignore" &&
            position + 1 < tokens.Count && tokens[position + 1] == "Case")
        {
            ignoreCase = true;
            position += 2;
        }

        var condition = new Condition(propertyName, op, isOr, ignoreCase);

        return ParseResult.Ok(new ParsedQuery
        {
            Conditions = new List<Condition> { condition }
        });
    }

    /// <summary>
    /// Attempts to match a property name from the token list, supporting multi-token compound property names.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated on a successful match.</param>
    /// <param name="properties">The entity properties available for matching.</param>
    /// <returns>The matched property name, or <c>null</c> if no property could be matched.</returns>
    private string? FindProperty(List<string> tokens, ref int position, List<string> properties)
    {
        for (int length = 3; length >= 1; length--)
        {
            if (position + length > tokens.Count)
                continue;

            var combined = string.Join("", tokens.Skip(position).Take(length));

            if (properties.Contains(combined, StringComparer.OrdinalIgnoreCase))
            {
                position += length;
                return combined;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to parse an operator keyword from the token list, supporting multi-token operators.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated on a successful match.</param>
    /// <returns>The matched <see cref="Operator"/>, or <c>null</c> if no operator keyword was found.</returns>
    private Operator? TryParseOperator(List<string> tokens, ref int position)
    {
        for (int length = 3; length >= 1; length--)
        {
            if (position + length > tokens.Count)
                continue;

            var combined = string.Join("", tokens.Skip(position).Take(length));

            if (OperatorKeywords.TryGetValue(combined, out var op))
            {
                position += length;
                return op;
            }
        }

        return null;
    }

    /// <summary>
    /// Parses the OrderBy clause from the token list.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The current position in the token list, updated as tokens are consumed.</param>
    /// <param name="properties">The entity properties available for matching.</param>
    /// <returns>A parse result containing the parsed OrderBy specification.</returns>
    internal ParseResult ParseOrderBy(List<string> tokens, ref int position, List<string> properties)
    {
        var orderByList = new List<OrderBy>();

        if (position >= tokens.Count || tokens[position] != "Order")
        {
            return ParseResult.Error("Expected 'Order'", position);
        }
        position++;

        if (position >= tokens.Count || tokens[position] != "By")
        {
            return ParseResult.Error("Expected 'By' after 'Order'", position);
        }
        position++;

        var propertyResult = FindProperty(tokens, ref position, properties);
        if (propertyResult == null)
        {
            return ParseResult.Error("Expected property name after 'OrderBy'", position);
        }

        var descending = false;

        if (position < tokens.Count)
        {
            if (tokens[position] == "Desc")
            {
                descending = true;
                position++;
            }
            else if (tokens[position] == "Asc")
            {
                descending = false;
                position++;
            }
        }

        orderByList.Add(new OrderBy(propertyResult, descending));

        return ParseResult.Ok(new ParsedQuery
        {
            OrderBy = orderByList
        });
    }

    /// <summary>
    /// Consumes a sequence of expected tokens starting at the given position and returns the new position.
    /// </summary>
    /// <param name="tokens">The full token list.</param>
    /// <param name="position">The starting position.</param>
    /// <param name="expected">The sequence of token values expected to be consumed in order.</param>
    /// <returns>The position after consuming all matching tokens.</returns>
    internal int ConsumeTokens(List<string> tokens, int position, params string[] expected)
    {
        foreach (var exp in expected)
        {
            if (position >= tokens.Count || tokens[position] != exp)
            {
                return position;
            }
            position++;
        }
        return position;
    }
}

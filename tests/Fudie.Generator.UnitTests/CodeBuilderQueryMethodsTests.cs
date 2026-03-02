namespace Fudie.Generator.UnitTests;

/// <summary>
/// Tests para la generación de query methods en CodeBuilder
/// </summary>
public class CodeBuilderQueryMethodsTests
{
    [Fact]
    public void GenerateQueryMethods_WithNullQueryMethods_ShouldReturnEmpty()
    {
        // Act
        var result = CodeBuilder.GenerateQueryMethods(null!, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithEmptyQueryMethods_ShouldReturnEmpty()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>();

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithFailedParseResult_ShouldSkipMethod()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByInvalid",
                ParseResult = ParseResult.Error("Invalid method name"),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GenerateQueryMethods_WithSimpleFindBy_ShouldGenerateCorrectCode()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<List<User>> FindByEmail(string email)", result);
        Assert.Contains("return await _query.Query<User>()", result);
        Assert.Contains(".Where(x => x.Email == email)", result);
        Assert.Contains(".ToListAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithFindFirstBy_ShouldGenerateNullableReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindFirstByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<User?> FindFirstByEmail(string email)", result);
        Assert.Contains(".FirstOrDefaultAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithCountBy_ShouldGenerateIntReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "CountByActiveTrue",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<int> CountByActiveTrue()", result);
        Assert.Contains(".Where(x => x.Active == true)", result);
        Assert.Contains(".CountAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithExistsBy_ShouldGenerateBoolReturn()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "ExistsByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<bool> ExistsByEmail(string email)", result);
        Assert.Contains(".AnyAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithDeleteBy_ShouldGenerateDeleteCode()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition>
            {
                new("Active", Operator.False)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "DeleteByActiveFalse",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("public async Task<int> DeleteByActiveFalse()", result);
        Assert.Contains(".ExecuteDeleteAsync()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithMultipleMethods_ShouldGenerateAll()
    {
        // Arrange
        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByEmail",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Find,
                    Conditions = new List<Condition> { new("Email", Operator.Equal) }
                }),
                Parameters = new() { ("email", "string") }
            },
            new()
            {
                MethodName = "CountByActiveTrue",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Count,
                    Conditions = new List<Condition> { new("Active", Operator.True) }
                }),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("FindByEmail", result);
        Assert.Contains("CountByActiveTrue", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithComplexQuery_ShouldGenerateAllParts()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan),
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: true)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("age", "int") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc", result);
        Assert.Contains("int age", result);
        Assert.Contains("x.Age > age && x.Active == true", result);
        Assert.Contains(".OrderByDescending(x => x.CreatedAt)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithTop_ShouldGenerateTake()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 10,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindTop10ByActiveTrue",
                ParseResult = ParseResult.Ok(query),
                Parameters = new()
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains(".Take(10)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithMultipleParameters_ShouldIncludeAll()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByNameAndAge",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string"), ("age", "int") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("string name, int age", result);
        Assert.Contains("x.Name == name && x.Age == age", result);
    }

    #region Tracking Tests

    [Fact]
    public void GenerateQueryMethods_WithUseTrackingFalse_ShouldUseQueryQuery()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Id", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindById",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("id", "Guid") },
                UseTracking = false
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("_query.Query<User>()", result);
        Assert.DoesNotContain("_entityLookup", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithUseTrackingTrue_ShouldUseEntityLookupSet()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Id", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindById",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("id", "Guid") },
                UseTracking = true
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert
        Assert.Contains("_entityLookup.Set<User>()", result);
        Assert.DoesNotContain("_query.Query", result);
    }

    [Fact]
    public void GenerateQueryMethods_DefaultTracking_ShouldUseQueryQuery()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
                // UseTracking no especificado = false por defecto
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User");

        // Assert - default should use _query.Query (no tracking)
        Assert.Contains("_query.Query<User>()", result);
    }

    [Fact]
    public void GenerateQueryMethods_MixedTracking_ShouldGenerateBothSources()
    {
        // Arrange - Simula la combinación problemática del usuario
        var queryNoTracking = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var queryWithTracking = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition> { new("Id", Operator.Equal), new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(queryNoTracking),
                Parameters = new() { ("name", "string") },
                UseTracking = false  // [AsNoTracking]
            },
            new()
            {
                MethodName = "FindFirstByIdAndName",
                ParseResult = ParseResult.Ok(queryWithTracking),
                Parameters = new() { ("id", "Guid"), ("name", "string") },
                UseTracking = true  // [Tracking]
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Ingredient");

        // Assert - debe generar ambos tipos de query
        Assert.Contains("_query.Query<Ingredient>()", result);  // FindByName usa IQuery
        Assert.Contains("_entityLookup.Set<Ingredient>()", result);  // FindFirstByIdAndName usa IEntityLookup
    }

    #endregion

    #region Config Modifier Tests

    [Fact]
    public void GenerateQueryMethods_WithIgnoreQueryFiltersConfig_ShouldApplyIgnoreQueryFilters()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IgnoreQueryFilters = true
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User", config);

        // Assert
        Assert.Contains(".IgnoreQueryFilters()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithAsSplitQueryConfig_ShouldApplyAsSplitQuery()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Email", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByEmail",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("email", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            AsSplitQuery = true
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User", config);

        // Assert
        Assert.Contains(".AsSplitQuery()", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithIncludePathsConfig_ShouldApplyIncludes()
    {
        // Arrange
        var testData = TestHelper.CreateTestCompilation();
        var path = PathValidator.ValidatePath("Orders", testData.customerSymbol, testData.compilation);

        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { path }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Customer", config);

        // Assert
        Assert.Contains(".Include(c => c.Orders)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithNestedIncludePathsConfig_ShouldApplyThenInclude()
    {
        // Arrange
        var testData = TestHelper.CreateTestCompilation();
        var path = PathValidator.ValidatePath("Orders.OrderItems", testData.customerSymbol, testData.compilation);

        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { path }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Customer", config);

        // Assert
        Assert.Contains(".Include(c => c.Orders)", result);
        Assert.Contains(".ThenInclude(o => o.OrderItems)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithAllConfigModifiers_ShouldApplyAll()
    {
        // Arrange
        var testData = TestHelper.CreateTestCompilation();
        var path = PathValidator.ValidatePath("Orders", testData.customerSymbol, testData.compilation);

        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") },
                UseTracking = true
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { path },
            IgnoreQueryFilters = true,
            AsSplitQuery = true
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Customer", config);

        // Assert
        Assert.Contains("_entityLookup.Set<Customer>()", result);
        Assert.Contains(".IgnoreQueryFilters()", result);
        Assert.Contains(".Include(c => c.Orders)", result);
        Assert.Contains(".AsSplitQuery()", result);
        Assert.Contains(".Where(x => x.Name == name)", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithNullConfig_ShouldNotApplyModifiers()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User", null);

        // Assert
        Assert.DoesNotContain("IgnoreQueryFilters", result);
        Assert.DoesNotContain("AsSplitQuery", result);
        Assert.DoesNotContain("Include", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithEmptyIncludePaths_ShouldNotApplyIncludes()
    {
        // Arrange
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = Array.Empty<PathValidator.IncludePathInfo>()
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User", config);

        // Assert
        Assert.DoesNotContain("Include", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithValidPathButZeroSegments_ShouldSkipInclude()
    {
        // Arrange - IncludePathInfo that is Valid but has 0 SegmentDetails
        // This exercises the "if (pathInfo.SegmentDetails.Length == 0) continue;" branch in BuildIncludeChains
        var emptySegmentsPath = new PathValidator.IncludePathInfo(
            OriginalPath: "Empty",
            Segments: Array.Empty<string>(),
            SegmentDetails: Array.Empty<PathValidator.SegmentInfo>(),
            IsValid: true,
            ErrorMessage: null,
            Location: null);

        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(query),
                Parameters = new() { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { emptySegmentsPath }
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "User", config);

        // Assert - No includes should be generated
        Assert.DoesNotContain("Include", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithDeepIncludePath_ShouldCoverNonCollectionNavigation()
    {
        // Arrange - path with non-collection navigation (Product.Category) to cover prev.IsCollection=false in BuildIncludeChains
        var testData = TestHelper.CreateTestCompilation();
        var deepPath = PathValidator.ValidatePath("Orders.OrderItems.Product.Category", testData.customerSymbol, testData.compilation);

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Find,
                    First = true,
                    Conditions = new List<Condition> { new("Name", Operator.Equal) }
                }),
                Parameters = new List<(string name, string type)> { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { deepPath },
            QueryMethods = queryMethods
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Customer", config);

        // Assert
        Assert.Contains(".Include(", result);
        Assert.Contains(".ThenInclude(", result);
    }

    [Fact]
    public void GenerateQueryMethods_WithValidPathButEmptySegments_ShouldNotGenerateIncludes()
    {
        // Arrange - IncludePathInfo valid but with empty SegmentDetails
        var emptySegmentPath = new PathValidator.IncludePathInfo(
            "EmptyPath", Array.Empty<string>(), Array.Empty<PathValidator.SegmentInfo>(),
            true, null, null);

        var queryMethods = new List<CodeBuilder.QueryMethodInfo>
        {
            new()
            {
                MethodName = "FindByName",
                ParseResult = ParseResult.Ok(new ParsedQuery
                {
                    Type = QueryType.Find,
                    First = true,
                    Conditions = new List<Condition> { new("Name", Operator.Equal) }
                }),
                Parameters = new List<(string name, string type)> { ("name", "string") }
            }
        };

        var config = new CodeBuilder.RepositoryConfig
        {
            IncludePaths = new[] { emptySegmentPath },
            QueryMethods = queryMethods
        };

        // Act
        var result = CodeBuilder.GenerateQueryMethods(queryMethods, "Customer", config);

        // Assert
        Assert.Contains("FindByName", result);
        Assert.DoesNotContain(".Include(", result);
    }

    #endregion
}

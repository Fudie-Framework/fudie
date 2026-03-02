namespace Fudie.Generator.UnitTests.QueryMethod;

/// <summary>
/// Tests para el generador de código LINQ
/// </summary>
public class LinqEmitterTests
{
    #region Simple Queries Tests

    [Fact]
    public void Emit_FindByName_ShouldGenerateWhereAndToListAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" });

        // Assert
        Assert.Contains("_query.Query<User>()", code);
        Assert.Contains(".Where(x => x.Name == name)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    [Fact]
    public void Emit_FindFirstByEmail_ShouldGenerateFirstOrDefaultAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindFirstByEmail", "User", new[] { "email" });

        // Assert
        Assert.Contains(".Where(x => x.Email == email)", code);
        Assert.Contains(".FirstOrDefaultAsync()", code);
        Assert.DoesNotContain(".ToListAsync()", code);
    }

    [Fact]
    public void Emit_CountByActiveTrue_ShouldGenerateCountAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var code = emitter.Emit(query, "CountByActiveTrue", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Where(x => x.Active == true)", code);
        Assert.Contains(".CountAsync()", code);
    }

    [Fact]
    public void Emit_ExistsByEmail_ShouldGenerateAnyAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "ExistsByEmail", "User", new[] { "email" });

        // Assert
        Assert.Contains(".Where(x => x.Email == email)", code);
        Assert.Contains(".AnyAsync()", code);
    }

    [Fact]
    public void Emit_DeleteByActiveFalse_ShouldGenerateExecuteDeleteAsync()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition>
            {
                new("Active", Operator.False)
            }
        };

        // Act
        var code = emitter.Emit(query, "DeleteByActiveFalse", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Where(x => x.Active == false)", code);
        Assert.Contains(".ExecuteDeleteAsync()", code);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Emit_NotEqualOperator_ShouldGenerateNotEqual()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.NotEqual)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusNot", "User", new[] { "status" });

        // Assert
        Assert.Contains("x.Status != status", code);
    }

    [Fact]
    public void Emit_LessThanOperator_ShouldGenerateLessThan()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.LessThan)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeLessThan", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age < age", code);
    }

    [Fact]
    public void Emit_GreaterThanOperator_ShouldGenerateGreaterThan()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeGreaterThan", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age > age", code);
    }

    [Fact]
    public void Emit_BetweenOperator_ShouldGenerateBetweenCondition()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.Between)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeBetween", "User", new[] { "min", "max" });

        // Assert
        Assert.Contains("x.Age >= min && x.Age <= max", code);
    }

    [Fact]
    public void Emit_InOperator_ShouldGenerateContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.In)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusIn", "User", new[] { "statuses" });

        // Assert
        Assert.Contains("statuses.Contains(x.Status)", code);
    }

    [Fact]
    public void Emit_NotInOperator_ShouldGenerateNotContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Status", Operator.NotIn)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByStatusNotIn", "User", new[] { "statuses" });

        // Assert
        Assert.Contains("!statuses.Contains(x.Status)", code);
    }

    [Fact]
    public void Emit_StartsWithOperator_ShouldGenerateStartsWith()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.StartsWith)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameStartingWith", "User", new[] { "prefix" });

        // Assert
        Assert.Contains("x.Name.StartsWith(prefix)", code);
    }

    [Fact]
    public void Emit_EndsWithOperator_ShouldGenerateEndsWith()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.EndsWith)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameEndingWith", "User", new[] { "suffix" });

        // Assert
        Assert.Contains("x.Name.EndsWith(suffix)", code);
    }

    [Fact]
    public void Emit_ContainsOperator_ShouldGenerateContains()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Contains)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameContaining", "User", new[] { "text" });

        // Assert
        Assert.Contains("x.Name.Contains(text)", code);
    }

    [Fact]
    public void Emit_LikeOperator_ShouldGenerateEFLike()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Like)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameLike", "User", new[] { "pattern" });

        // Assert
        Assert.Contains("EF.Functions.Like(x.Name, pattern)", code);
    }

    [Fact]
    public void Emit_IsNullOperator_ShouldGenerateNullCheck()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Description", Operator.IsNull)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByDescriptionIsNull", "User", Array.Empty<string>());

        // Assert
        Assert.Contains("x.Description == null", code);
    }

    [Fact]
    public void Emit_IsNotNullOperator_ShouldGenerateNotNullCheck()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Description", Operator.IsNotNull)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByDescriptionIsNotNull", "User", Array.Empty<string>());

        // Assert
        Assert.Contains("x.Description != null", code);
    }

    #endregion

    #region Logical Operators Tests

    [Fact]
    public void Emit_AndConditions_ShouldGenerateAndOperator()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameAndAge", "User", new[] { "name", "age" });

        // Assert
        Assert.Contains("x.Name == name && x.Age == age", code);
    }

    [Fact]
    public void Emit_OrConditions_ShouldGenerateOrOperator()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Email", Operator.Equal, Or: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameOrEmail", "User", new[] { "name", "email" });

        // Assert
        Assert.Contains("x.Name == name || x.Email == email", code);
    }

    [Fact]
    public void Emit_MixedAndOr_ShouldGenerateCorrectPrecedence()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal),
                new("Status", Operator.Equal, Or: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameAndAgeOrStatus", "User", new[] { "name", "age", "status" });

        // Assert
        Assert.Contains("x.Name == name && x.Age == age || x.Status == status", code);
    }

    #endregion

    #region IgnoreCase Tests

    [Fact]
    public void Emit_IgnoreCase_ShouldGenerateToLower()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal, IgnoreCase: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByEmailIgnoreCase", "User", new[] { "email" });

        // Assert
        Assert.Contains("x.Email.ToLower() == email.ToLower()", code);
    }

    #endregion

    #region OrderBy Tests

    [Fact]
    public void Emit_OrderByAscending_ShouldGenerateOrderBy()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: false)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByActiveTrueOrderByCreatedAt", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".OrderBy(x => x.CreatedAt)", code);
    }

    [Fact]
    public void Emit_OrderByDescending_ShouldGenerateOrderByDescending()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            },
            OrderBy = new List<OrderBy>
            {
                new("Score", Descending: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByActiveTrueOrderByScoreDesc", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".OrderByDescending(x => x.Score)", code);
    }

    #endregion

    #region Top Tests

    [Fact]
    public void Emit_Top10_ShouldGenerateTake()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 10,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindTop10ByActiveTrue", "User", Array.Empty<string>());

        // Assert
        Assert.Contains(".Take(10)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public void Emit_ComplexQuery_ShouldGenerateAllParts()
    {
        // Arrange
        var emitter = new LinqEmitter();
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

        // Act
        var code = emitter.Emit(query, "FindByAgeGreaterThanAndActiveTrueOrderByCreatedAtDesc", "User", new[] { "age" });

        // Assert
        Assert.Contains("_query.Query<User>()", code);
        Assert.Contains(".Where(x => x.Age > age && x.Active == true)", code);
        Assert.Contains(".OrderByDescending(x => x.CreatedAt)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    #endregion

    #region Method Signature Tests

    [Fact]
    public void EmitMethodSignature_FindBy_ShouldGenerateCorrectSignature()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "FindByName", "User", new[] { ("name", "string") });

        // Assert
        Assert.Contains("public async Task<List<User>> FindByName(string name)", signature);
    }

    [Fact]
    public void EmitMethodSignature_FindFirstBy_ShouldGenerateNullableReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "FindFirstByEmail", "User", new[] { ("email", "string") });

        // Assert
        Assert.Contains("public async Task<User?> FindFirstByEmail(string email)", signature);
    }

    [Fact]
    public void EmitMethodSignature_CountBy_ShouldGenerateIntReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "CountByActiveTrue", "User", Array.Empty<(string, string)>());

        // Assert
        Assert.Contains("public async Task<int> CountByActiveTrue()", signature);
    }

    [Fact]
    public void EmitMethodSignature_ExistsBy_ShouldGenerateBoolReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition> { new("Email", Operator.Equal) }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "ExistsByEmail", "User", new[] { ("email", "string") });

        // Assert
        Assert.Contains("public async Task<bool> ExistsByEmail(string email)", signature);
    }

    [Fact]
    public void EmitMethodSignature_DeleteBy_ShouldGenerateIntReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "DeleteByActiveFalse", "User", Array.Empty<(string, string)>());

        // Assert
        Assert.Contains("public async Task<int> DeleteByActiveFalse()", signature);
    }

    [Fact]
    public void EmitMethodSignature_FindTop1_ShouldGenerateNullableReturn()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 1,
            Conditions = new List<Condition> { new("Email", Operator.Equal) }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "FindTop1ByEmail", "User", new[] { ("email", "string") });

        // Assert
        Assert.Contains("Task<User?>", signature);
    }

    #endregion

    #region Modifier Tests

    [Fact]
    public void Emit_WithIgnoreQueryFilters_ShouldGenerateIgnoreQueryFilters()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" },
            ignoreQueryFilters: true);

        // Assert
        Assert.Contains(".IgnoreQueryFilters()", code);
    }

    [Fact]
    public void Emit_WithAsSplitQuery_ShouldGenerateAsSplitQuery()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" },
            asSplitQuery: true);

        // Assert
        Assert.Contains(".AsSplitQuery()", code);
    }

    [Fact]
    public void Emit_WithIncludeChains_ShouldGenerateIncludes()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var includes = new[] { ".Include(c => c.Orders)", ".Include(c => c.Address)" };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" },
            includeChains: includes);

        // Assert
        Assert.Contains(".Include(c => c.Orders)", code);
        Assert.Contains(".Include(c => c.Address)", code);
    }

    [Fact]
    public void Emit_WithAllModifiers_ShouldGenerateInCorrectOrder()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var includes = new[] { ".Include(c => c.Orders)" };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" },
            useTracking: true,
            ignoreQueryFilters: true,
            asSplitQuery: true,
            includeChains: includes);

        // Assert
        Assert.Contains("_entityLookup.Set<User>()", code);
        var ignoreIdx = code.IndexOf("IgnoreQueryFilters");
        var includeIdx = code.IndexOf("Include");
        var splitIdx = code.IndexOf("AsSplitQuery");
        var whereIdx = code.IndexOf("Where");

        Assert.True(ignoreIdx < includeIdx, "IgnoreQueryFilters should be before Include");
        Assert.True(includeIdx < splitIdx, "Include should be before AsSplitQuery");
        Assert.True(splitIdx < whereIdx, "AsSplitQuery should be before Where");
    }

    [Fact]
    public void Emit_WithUseTracking_ShouldUseEntityLookupSet()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" }, useTracking: true);

        // Assert
        Assert.Contains("_entityLookup.Set<User>()", code);
        Assert.DoesNotContain("_query.Query", code);
    }

    #endregion

    #region EmitFullMethod Tests

    [Fact]
    public void EmitFullMethod_ShouldGenerateCompleteMethod()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Email", Operator.Equal) }
        };

        // Act
        var code = emitter.EmitFullMethod(query, "FindByEmail", "User", new[] { ("email", "string") });

        // Assert
        Assert.Contains("public async Task<List<User>> FindByEmail(string email)", code);
        Assert.Contains("{", code);
        Assert.Contains("return await", code);
        Assert.Contains("_query.Query<User>()", code);
        Assert.Contains(".Where(x => x.Email == email)", code);
        Assert.Contains(".ToListAsync()", code);
        Assert.Contains("}", code);
    }

    [Fact]
    public void EmitFullMethod_WithFirstQuery_ShouldReturnNullable()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition> { new("Id", Operator.Equal) }
        };

        // Act
        var code = emitter.EmitFullMethod(query, "FindFirstById", "User", new[] { ("id", "Guid") });

        // Assert
        Assert.Contains("public async Task<User?> FindFirstById(Guid id)", code);
        Assert.Contains(".FirstOrDefaultAsync()", code);
    }

    [Fact]
    public void EmitFullMethod_WithDelete_ShouldReturnInt()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };

        // Act
        var code = emitter.EmitFullMethod(query, "DeleteByActiveFalse", "User", Array.Empty<(string, string)>());

        // Assert
        Assert.Contains("public async Task<int> DeleteByActiveFalse()", code);
        Assert.Contains(".ExecuteDeleteAsync()", code);
    }

    #endregion

    #region IgnoreCase with Other Operators Tests

    [Fact]
    public void Emit_NotEqualWithIgnoreCase_ShouldGenerateToLower()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.NotEqual, IgnoreCase: true)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByNameNotIgnoreCase", "User", new[] { "name" });

        // Assert
        Assert.Contains("x.Name.ToLower() != name.ToLower()", code);
    }

    #endregion

    #region LessThanOrEqual and GreaterThanOrEqual Tests

    [Fact]
    public void Emit_LessThanOrEqualOperator_ShouldGenerateLessThanOrEqual()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.LessThanOrEqual)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeLessThanEqual", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age <= age", code);
    }

    [Fact]
    public void Emit_GreaterThanOrEqualOperator_ShouldGenerateGreaterThanOrEqual()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThanOrEqual)
            }
        };

        // Act
        var code = emitter.Emit(query, "FindByAgeGreaterThanEqual", "User", new[] { "age" });

        // Assert
        Assert.Contains("x.Age >= age", code);
    }

    #endregion

    #region No Conditions Tests

    [Fact]
    public void Emit_WithNoConditions_ShouldNotGenerateWhere()
    {
        // Arrange
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            OrderBy = new List<OrderBy> { new("Name", Descending: false) }
        };

        // Act
        var code = emitter.Emit(query, "FindAllOrderByName", "User", Array.Empty<string>());

        // Assert
        Assert.DoesNotContain(".Where(", code);
        Assert.Contains(".OrderBy(x => x.Name)", code);
        Assert.Contains(".ToListAsync()", code);
    }

    #endregion

    #region Default/Edge Case Coverage

    [Fact]
    public void EmitMethodSignature_WithUnknownQueryType_ShouldReturnTask()
    {
        // Arrange - cast invalid int to QueryType to hit the default branch
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = (QueryType)999,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        // Act
        var signature = emitter.EmitMethodSignature(query, "Test", "User", new[] { ("name", "string") });

        // Assert - default case returns "Task"
        Assert.Contains("Task Test(string name)", signature);
    }

    [Fact]
    public void Emit_WithUnknownOperator_ShouldThrowInvalidOperationException()
    {
        // Arrange - cast invalid int to Operator to hit the default throw
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", (Operator)999) }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            emitter.Emit(query, "Test", "User", new[] { "name" }));
    }

    [Fact]
    public void Emit_WithEmptyOrderByList_ShouldNotGenerateOrderBy()
    {
        // Arrange - empty OrderBy list exercises GenerateOrderByExpression Count == 0 guard
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) },
            OrderBy = new List<OrderBy>() // empty list
        };

        // Act
        var code = emitter.Emit(query, "FindByName", "User", new[] { "name" });

        // Assert
        Assert.DoesNotContain("OrderBy", code);
    }

    [Fact]
    public void Emit_WithUnknownQueryType_ShouldThrowInvalidOperationException()
    {
        // Arrange - cast invalid int to QueryType to hit GenerateFinalExecution default throw
        var emitter = new LinqEmitter();
        var query = new ParsedQuery
        {
            Type = (QueryType)999,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            emitter.Emit(query, "Test", "User", new[] { "name" }));
    }

    [Fact]
    public void GenerateOrderByExpression_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange - defensive guard: callers beyond Emit should be safe
        var emitter = new LinqEmitter();

        // Act
        var result = emitter.GenerateOrderByExpression(new List<OrderBy>());

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion
}

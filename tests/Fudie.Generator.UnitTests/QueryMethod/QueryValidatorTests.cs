namespace Fudie.Generator.UnitTests.QueryMethod;

/// <summary>
/// Tests para el validador de queries
/// </summary>
public class QueryValidatorTests
{
    private const string TestEntityCode = @"
        namespace TestNamespace
        {
            public class User
            {
                public System.Guid Id { get; set; }
                public string Name { get; set; }
                public string Email { get; set; }
                public int Age { get; set; }
                public bool Active { get; set; }
                public System.DateTime CreatedAt { get; set; }
                public UserStatus Status { get; set; }
                public string? Description { get; set; }
            }

            public enum UserStatus
            {
                Active,
                Inactive,
                Pending
            }

            public interface IUserRepository
            {
                System.Threading.Tasks.Task<User?> FindByEmail(string email);
                System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByName(string name);
                System.Threading.Tasks.Task<int> CountByActiveTrue();
                System.Threading.Tasks.Task<bool> ExistsByEmail(string email);
            }
        }";

    private (Compilation compilation, INamedTypeSymbol entityType, INamedTypeSymbol interfaceType) GetTestCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(TestEntityCode);
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location)
            });

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        var userClass = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "User");

        var interfaceDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>()
            .First(i => i.Identifier.Text == "IUserRepository");

        var entityType = semanticModel.GetDeclaredSymbol(userClass) as INamedTypeSymbol;
        var interfaceType = semanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;

        return (compilation, entityType!, interfaceType!);
    }

    #region Property Existence Tests

    [Fact]
    public void Validate_ExistingProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_NonExistingProperty_ShouldReportREPO001()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Emial", Operator.Equal) // Typo: should be "Email"
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Single(diagnostics);
        Assert.Equal("REPO001", diagnostics[0].Id);
        Assert.Contains("Emial", diagnostics[0].GetMessage());
        Assert.Contains("Email", diagnostics[0].GetMessage()); // Should suggest "Email"
    }

    [Fact]
    public void Validate_PropertyInOrderBy_ShouldValidate()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            },
            OrderBy = new List<OrderBy>
            {
                new("CreatedAt", Descending: true)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_NonExistingPropertyInOrderBy_ShouldReportREPO001()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            },
            OrderBy = new List<OrderBy>
            {
                new("UpdatedAt", Descending: true) // Doesn't exist
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Single(diagnostics);
        Assert.Equal("REPO001", diagnostics[0].Id);
        Assert.Contains("UpdatedAt", diagnostics[0].GetMessage());
    }

    #endregion

    #region Parameter Count Tests

    [Fact]
    public void Validate_CorrectParameterCount_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_MissingParameter_ShouldReportREPO003()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();

        // Query expects 2 parameters (Name and Age) but method only has 1
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal),
                new("Age", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO003");
    }

    [Fact]
    public void Validate_OperatorWithNoParameters_ShouldNotRequireParameter()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True) // True operator requires no parameter
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    #endregion

    #region Return Type Tests

    [Fact]
    public void Validate_FindByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByName").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_FindFirstByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_CountByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ExistsByWithCorrectReturnType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("ExistsByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Empty(diagnostics);
    }

    #endregion

    #region Operator Compatibility Tests

    [Fact]
    public void Validate_NumericOperatorOnNumericProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Age", Operator.GreaterThan)
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByAgeGreaterThan(int age);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_NumericOperatorOnStringProperty_ShouldReportREPO006()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.GreaterThan) // GreaterThan not valid for string
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameGreaterThan(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_StringOperatorOnStringProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.StartsWith)
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameStartingWith(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BooleanOperatorOnBooleanProperty_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition>
            {
                new("Active", Operator.True)
            }
        };

        var method = interfaceType.GetMembers("CountByActiveTrue").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BooleanOperatorOnNonBooleanProperty_ShouldReportREPO006()
    {
        // Arrange
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Name", Operator.True) // True operator only valid for bool
            }
        };

        // Create a mock method
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameTrue();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var methodDecl = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();
        var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    #endregion

    #region Type Compatibility Tests

    [Fact]
    public void Validate_MatchingParameterType_ShouldNotReportError()
    {
        // Arrange
        var (compilation, entityType, interfaceType) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition>
            {
                new("Email", Operator.Equal)
            }
        };

        var method = interfaceType.GetMembers("FindByEmail").First() as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    #endregion

    #region Return Type Validation Tests

    [Fact]
    public void Validate_FindByWithWrongReturnType_ShouldReportREPO005()
    {
        // Arrange - Method returns Task<string> instead of Task<User?> or Task<List<User>>
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<string> FindByName(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_CountByWithWrongReturnType_ShouldReportREPO005()
    {
        // Arrange - Method returns Task<string> instead of Task<int>
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition> { new("Active", Operator.True) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<string> CountByActiveTrue();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_ExistsByWithWrongReturnType_ShouldReportREPO005()
    {
        // Arrange - Method returns Task<int> instead of Task<bool>
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition> { new("Email", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<int> ExistsByEmail(string email);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_DeleteByWithWrongReturnType_ShouldReportREPO005()
    {
        // Arrange - Method returns Task<string> instead of Task<int>
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<string> DeleteByActiveFalse();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_DeleteByWithTaskReturnType_ShouldNotReport()
    {
        // Arrange - Task (no generic) is valid for Delete
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task DeleteByActiveFalse();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_FindByWithTaskNoGenericReturnType_ShouldReportREPO005()
    {
        // Arrange - Task without generic is NOT valid for Find
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task FindByName(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_NonTaskReturnType_ShouldReportREPO005()
    {
        // Arrange - Method returns void (not Task)
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void FindByName(string name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    #endregion

    #region Extra Parameter Count Tests

    [Fact]
    public void Validate_ExtraParameters_ShouldReportREPO004()
    {
        // Arrange - Method has 2 params but query only needs 1
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByName(string name, int extra);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO004");
    }

    #endregion

    #region Parameter Type Validation Tests

    [Fact]
    public void Validate_InOperatorWithWrongCollectionType_ShouldReportREPO002()
    {
        // Arrange - In operator with string parameter instead of IEnumerable<T>
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.In) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameIn(string names);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO002");
    }

    [Fact]
    public void Validate_InOperatorWithCorrectCollectionType_ShouldNotReport()
    {
        // Arrange - In operator with List<string> parameter (implements IEnumerable<string>)
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.In) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByNameIn(System.Collections.Generic.List<string> names);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    [Fact]
    public void Validate_WrongParameterType_ShouldReportREPO002()
    {
        // Arrange - Method has int parameter for string property
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByName(int name);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO002");
    }

    [Fact]
    public void Validate_NumericTypeCompatibility_ShouldNotReport()
    {
        // Arrange - long parameter for int property (numeric compatibility)
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Age", Operator.GreaterThan) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByAgeGreaterThan(long age);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    #endregion

    #region Date Type Operator Compatibility Tests

    [Fact]
    public void Validate_NumericOperatorOnDateTimeProperty_ShouldNotReportError()
    {
        // Arrange - GreaterThan on DateTime property is valid
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("CreatedAt", Operator.GreaterThan) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByCreatedAtGreaterThan(System.DateTime date);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_StringOperatorOnNonStringProperty_ShouldReportREPO006()
    {
        // Arrange - StartsWith on int property
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Age", Operator.StartsWith) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByAgeStartingWith(int prefix);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BetweenOperatorOnDateTimeProperty_ShouldNotReportError()
    {
        // Arrange - Between on DateTime is valid
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("CreatedAt", Operator.Between) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByCreatedAtBetween(System.DateTime from, System.DateTime to);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_NullableParameterMatchingProperty_ShouldNotReport()
    {
        // Arrange - Nullable<int> param for int property
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Age", Operator.Equal) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> FindByAge(int? age);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    [Fact]
    public void Validate_CountByWithLongReturnType_ShouldNotReport()
    {
        // Arrange - Task<long> is valid for Count
        var (compilation, entityType, _) = GetTestCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition> { new("Active", Operator.True) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<long> CountByActiveTrue();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        // Act
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);

        // Assert
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO005");
    }

    #endregion

    #region DateOnly/TimeOnly/DateTimeOffset Type Tests

    private const string DateTypesEntityCode = @"
        namespace TestNamespace
        {
            public class Event
            {
                public System.Guid Id { get; set; }
                public string Name { get; set; }
                public System.DateOnly EventDate { get; set; }
                public System.TimeOnly StartTime { get; set; }
                public System.DateTimeOffset CreatedOffset { get; set; }
                public byte Priority { get; set; }
                public short Rank { get; set; }
                public float Score { get; set; }
                public double Rating { get; set; }
                public decimal Price { get; set; }
                public long BigNumber { get; set; }
                public bool Active { get; set; }
            }

            public interface IEventRepository
            {
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByEventDateGreaterThan(System.DateOnly date);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByStartTimeGreaterThan(System.TimeOnly time);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByCreatedOffsetGreaterThan(System.DateTimeOffset offset);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByPriorityGreaterThan(byte priority);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByRankGreaterThan(short rank);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByScoreGreaterThan(float score);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByRatingGreaterThan(double rating);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByPriceBetween(decimal min, decimal max);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByBigNumberLessThanEqual(long num);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameEndingWith(string suffix);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameContaining(string text);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameLike(string pattern);
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameIsNull();
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameIsNotNull();
                System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByActiveFalse();
            }
        }";

    private (Compilation compilation, INamedTypeSymbol entityType, INamedTypeSymbol interfaceType) GetDateTypesCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(DateTypesEntityCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.DateOnly).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();

        var eventClass = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "Event");

        var interfaceDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>()
            .First(i => i.Identifier.Text == "IEventRepository");

        var entityType = semanticModel.GetDeclaredSymbol(eventClass) as INamedTypeSymbol;
        var interfaceType = semanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;

        return (compilation, entityType!, interfaceType!);
    }

    [Fact]
    public void Validate_GreaterThanOnDateOnlyProperty_ShouldNotReportREPO006()
    {
        var (compilation, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("EventDate", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByEventDateGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnTimeOnlyProperty_ShouldNotReportREPO006()
    {
        var (compilation, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("StartTime", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByStartTimeGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnDateTimeOffsetProperty_ShouldNotReportREPO006()
    {
        var (compilation, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("CreatedOffset", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByCreatedOffsetGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnByteProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Priority", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByPriorityGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnShortProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Rank", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByRankGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnFloatProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Score", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByScoreGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_GreaterThanOnDoubleProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Rating", Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers("FindByRatingGreaterThan").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_BetweenOnDecimalProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Price", Operator.Between) }
        };
        var method = interfaceType.GetMembers("FindByPriceBetween").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_LessThanEqualOnLongProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("BigNumber", Operator.LessThanOrEqual) }
        };
        var method = interfaceType.GetMembers("FindByBigNumberLessThanEqual").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_FalseOperatorOnBoolProperty_ShouldNotReportREPO006()
    {
        var (_, entityType, interfaceType) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };
        var method = interfaceType.GetMembers("FindByActiveFalse").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_FalseOperatorOnNonBoolProperty_ShouldReportREPO006()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.False) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByNameFalse();
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_EndingWithOnNonStringProperty_ShouldReportREPO006()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Priority", Operator.EndsWith) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByPriorityEndingWith(byte val);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_ContainingOnNonStringProperty_ShouldReportREPO006()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Priority", Operator.Contains) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByPriorityContaining(byte val);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_LikeOnNonStringProperty_ShouldReportREPO006()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Priority", Operator.Like) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByPriorityLike(byte val);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_LessThanOnGuidProperty_ShouldReportREPO006()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Id", Operator.LessThan) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByIdLessThan(System.Guid id);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO006");
    }

    [Fact]
    public void Validate_FloatParameterForDoubleProperty_ShouldNotReport()
    {
        var (compilation, entityType, _) = GetDateTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Rating", Operator.GreaterThan) }
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<Event>> FindByRatingGreaterThan(float score);
                }
            }");

        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First()) as IMethodSymbol;

        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    #endregion

    #region Coverage Gap Tests — Extended Entity

    private const string ExtendedTypesEntityCode = @"
        namespace TestNamespace
        {
            public class ExtendedEntity
            {
                public System.Guid Id { get; set; }
                public string Name { get; set; }
                public int Age { get; set; }
                public bool Active { get; set; }
                public sbyte SmallNum { get; set; }
                public ushort UShortNum { get; set; }
                public uint UIntNum { get; set; }
                public ulong ULongNum { get; set; }
                public byte ByteNum { get; set; }
                public short ShortNum { get; set; }
                public float FloatNum { get; set; }
                public double DoubleNum { get; set; }
                public decimal DecimalNum { get; set; }
            }

            public interface IExtendedRepository
            {
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindBySmallNumGreaterThan(sbyte val);
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByUShortNumGreaterThan(ushort val);
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByUIntNumGreaterThan(uint val);
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByULongNumGreaterThan(ulong val);
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByNameIsNull();
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByNameIsNotNull();
                System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByNameIn(System.Collections.Generic.IEnumerable<string> names);
            }
        }";

    private (Compilation compilation, INamedTypeSymbol entityType, INamedTypeSymbol interfaceType) GetExtendedTypesCompilation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(ExtendedTypesEntityCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var root = syntaxTree.GetRoot();
        var entityClass = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "ExtendedEntity");
        var interfaceDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax>()
            .First(i => i.Identifier.Text == "IExtendedRepository");
        var entityType = semanticModel.GetDeclaredSymbol(entityClass) as INamedTypeSymbol;
        var interfaceType = semanticModel.GetDeclaredSymbol(interfaceDecl) as INamedTypeSymbol;
        return (compilation, entityType!, interfaceType!);
    }

    // --- IsNumericOrDateType + IsNumericType: unsigned/signed types ---
    [Theory]
    [InlineData("SmallNum", "FindBySmallNumGreaterThan")]
    [InlineData("UShortNum", "FindByUShortNumGreaterThan")]
    [InlineData("UIntNum", "FindByUIntNumGreaterThan")]
    [InlineData("ULongNum", "FindByULongNumGreaterThan")]
    public void Validate_GreaterThanOnUnsignedTypes_ShouldNotReportREPO006(string property, string methodName)
    {
        var (_, entityType, interfaceType) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new(property, Operator.GreaterThan) }
        };
        var method = interfaceType.GetMembers(methodName).First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO006");
    }

    // --- GetParameterCountForOperator: IsNull, IsNotNull ---
    [Fact]
    public void Validate_IsNullOperator_ShouldRequireZeroParameters()
    {
        var (_, entityType, interfaceType) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.IsNull) }
        };
        var method = interfaceType.GetMembers("FindByNameIsNull").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO004");
    }

    [Fact]
    public void Validate_IsNotNullOperator_ShouldRequireZeroParameters()
    {
        var (_, entityType, interfaceType) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.IsNotNull) }
        };
        var method = interfaceType.GetMembers("FindByNameIsNotNull").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO004");
    }

    // --- IsEnumerableOf: direct IEnumerable<T> match ---
    [Fact]
    public void Validate_InOperatorWithDirectIEnumerable_ShouldNotReport()
    {
        var (_, entityType, interfaceType) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.In) }
        };
        var method = interfaceType.GetMembers("FindByNameIn").First() as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    // --- GetExpectedReturnType: Count, Exists, Delete with void return ---
    [Fact]
    public void Validate_CountByWithNonTaskReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Count,
            Conditions = new List<Condition> { new("Active", Operator.True) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void CountByActiveTrue();
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_ExistsByWithNonTaskReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Exists,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void ExistsByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_DeleteByWithNonTaskReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Delete,
            Conditions = new List<Condition> { new("Active", Operator.False) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void DeleteByActiveFalse();
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- GetExpectedReturnType: Find+First and Find+Top1 with void return ---
    [Fact]
    public void Validate_FindFirstByWithNonTaskReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void FindFirstByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_FindTop1ByWithNonTaskReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 1,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    void FindTop1ByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- ValidateReturnType line 217: array return type (not INamedTypeSymbol) ---
    [Fact]
    public void Validate_ArrayReturnType_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    int[] FindByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- ValidateReturnType line 258: FindFirst and FindTop1 with wrong Task<T> ---
    [Fact]
    public void Validate_FindFirstByWithWrongTaskGenericReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            First = true,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<string> FindFirstByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    [Fact]
    public void Validate_FindTop1ByWithWrongTaskGenericReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Top = 1,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<string> FindTop1ByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- IsNullableOf: Nullable<T> value type (lines 603-604) ---
    [Fact]
    public void Validate_FindByWithNullableValueReturnType_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<int?> FindByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- IsNullableOf: non-reference non-nullable value type (line 613) ---
    [Fact]
    public void Validate_FindByWithValueTypeReturn_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<int> FindByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    // --- AreTypesCompatible: non-numeric param (line 554) ---
    [Fact]
    public void Validate_BoolParamForGuidProperty_ShouldReportREPO002()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Id", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindById(bool id);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO002");
    }

    // --- UnwrapNullable: array param (not INamedTypeSymbol) ---
    [Fact]
    public void Validate_ArrayParamForStringProperty_ShouldReportREPO002()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByName(int[] name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO002");
    }

    // --- CalculateLevenshteinDistance: empty source (line 457) ---
    [Fact]
    public void Validate_EmptyPropertyName_ShouldReportREPO001()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO001");
    }

    // --- CalculateLevenshteinDistance: empty/null target (line 459-460) ---
    [Fact]
    public void CalculateLevenshteinDistance_WithNullTarget_ShouldReturnSourceLength()
    {
        var validator = new QueryValidator();
        var result = validator.CalculateLevenshteinDistance("abc", null!);
        Assert.Equal(3, result);
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithEmptyTarget_ShouldReturnSourceLength()
    {
        var validator = new QueryValidator();
        var result = validator.CalculateLevenshteinDistance("abc", "");
        Assert.Equal(3, result);
    }

    // --- GetExpectedReturnType: default branch ---
    [Fact]
    public void GetExpectedReturnType_WithUnknownQueryType_ShouldReturnTask()
    {
        var (_, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery { Type = (QueryType)999 };
        var result = validator.GetExpectedReturnType(query, entityType);
        Assert.Equal("Task", result);
    }

    // --- CalculateLevenshteinDistance: empty source + null target ---
    [Fact]
    public void CalculateLevenshteinDistance_WithEmptySourceAndNullTarget_ShouldReturnZero()
    {
        var validator = new QueryValidator();
        var result = validator.CalculateLevenshteinDistance("", null!);
        Assert.Equal(0, result);
    }

    // --- IsNumericType: cross-numeric type pairs to cover all switch arms ---
    [Theory]
    [InlineData("SmallNum")]
    [InlineData("UShortNum")]
    [InlineData("UIntNum")]
    [InlineData("ULongNum")]
    [InlineData("ByteNum")]
    [InlineData("ShortNum")]
    [InlineData("FloatNum")]
    [InlineData("DoubleNum")]
    [InlineData("DecimalNum")]
    public void Validate_CrossNumericTypeCompatibility_ShouldNotReportREPO002(string property)
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new(property, Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<System.Collections.Generic.List<ExtendedEntity>> FindByProp(long val);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.DoesNotContain(diagnostics, d => d.Id == "REPO002");
    }

    // --- IsNullableOf line 601: Task<int[]> to cover is-not-INamedTypeSymbol ---
    [Fact]
    public void Validate_FindByWithArrayTaskArgument_ShouldReportREPO005()
    {
        var (compilation, entityType, _) = GetExtendedTypesCompilation();
        var validator = new QueryValidator();
        var query = new ParsedQuery
        {
            Type = QueryType.Find,
            Conditions = new List<Condition> { new("Name", Operator.Equal) }
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
            namespace TestNamespace {
                public interface ITest {
                    System.Threading.Tasks.Task<int[]> FindByName(string name);
                }
            }");
        var testCompilation = compilation.AddSyntaxTrees(syntaxTree);
        var semanticModel = testCompilation.GetSemanticModel(syntaxTree);
        var method = semanticModel.GetDeclaredSymbol(syntaxTree.GetRoot()
            .DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First()) as IMethodSymbol;
        var diagnostics = validator.Validate(query, method!, entityType, Location.None);
        Assert.Contains(diagnostics, d => d.Id == "REPO005");
    }

    #endregion
}

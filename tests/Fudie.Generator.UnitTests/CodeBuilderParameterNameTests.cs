namespace Fudie.Generator.UnitTests;

public class CodeBuilderParameterNameTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public CodeBuilderParameterNameTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Parameter Name Generation

    [Fact]
    public void GenerateIncludeChain_WithCustomer_ShouldUseParameterC()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Customer", "query");

        // Assert
        result.Should().Contain("(c => c.Orders)");
    }

    [Fact]
    public void GenerateIncludeChain_WithOrder_ShouldUseParameterO()
    {
        // Arrange
        var path = PathValidator.ValidatePath("OrderItems", _testData.orderSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Order", "query");

        // Assert
        result.Should().Contain("(o => o.OrderItems)");
    }

    [Fact]
    public void GenerateIncludeChain_WithProduct_ShouldUseParameterP()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Category", _testData.productSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Product", "query");

        // Assert
        result.Should().Contain("(p => p.Category)");
    }

    [Fact]
    public void GenerateIncludeChain_WithNestedPath_ShouldUseCorrectParametersForEachLevel()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders.OrderItems.Product", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "Customer", "query");

        // Assert
        result.Should().Contain("(c => c.Orders)");
        result.Should().Contain("(o => o.OrderItems)");
        result.Should().Contain("(oi => oi.Product)");
    }

    [Fact]
    public void GenerateIncludeChain_WithOrderItem_ShouldUseTwoLetterParameter()
    {
        // Arrange - OrderItem debería generar 'oi' porque 'o' podría confundirse
        var path = PathValidator.ValidatePath("Product", _testData.orderItemSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "OrderItem", "query");

        // Assert
        result.Should().Contain("(oi => oi.Product)");
    }

    #endregion

    #region GenerateParameterName Null/Empty

    [Fact]
    public void GenerateIncludeChain_WithNullEntityName_ShouldUseXAsParameter()
    {
        // Arrange - passing null typeName should produce "x" as parameter name
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);

        // Act - use null! as entity name to exercise the null/whitespace branch
        var result = CodeBuilder.GenerateIncludeChain(path, null!, "query");

        // Assert - GenerateParameterName(null) returns "x"
        result.Should().Contain("(x => x.Orders)");
    }

    [Fact]
    public void GenerateIncludeChain_WithEmptyEntityName_ShouldUseXAsParameter()
    {
        // Arrange
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);

        // Act - use empty string as entity name to exercise the whitespace branch
        var result = CodeBuilder.GenerateIncludeChain(path, "", "query");

        // Assert - GenerateParameterName("") returns "x"
        result.Should().Contain("(x => x.Orders)");
    }

    [Fact]
    public void GenerateIncludeChain_WithSingleCharEntityName_ShouldUseLowercasedChar()
    {
        // Arrange - single char name exercises short-circuit in Length > 1 check
        var path = PathValidator.ValidatePath("Orders", _testData.customerSymbol, _testData.compilation);

        // Act
        var result = CodeBuilder.GenerateIncludeChain(path, "A", "query");

        // Assert
        result.Should().Contain("(a => a.Orders)");
    }

    #endregion

    #region QueryMethodsUseTracking Property

    [Fact]
    public void RepositoryConfig_QueryMethodsUseTracking_ShouldBeSettable()
    {
        // Arrange & Act
        var config = new CodeBuilder.RepositoryConfig
        {
            QueryMethodsUseTracking = true
        };

        // Assert
        config.QueryMethodsUseTracking.Should().BeTrue();
    }

    [Fact]
    public void RepositoryConfig_QueryMethodsUseTracking_DefaultShouldBeFalse()
    {
        // Arrange & Act
        var config = new CodeBuilder.RepositoryConfig();

        // Assert
        config.QueryMethodsUseTracking.Should().BeFalse();
    }

    #endregion
}

namespace Fudie.Generator.UnitTests;

public class PathValidatorEdgeCasesTests
{
    private readonly (Compilation compilation, INamedTypeSymbol customerSymbol, INamedTypeSymbol orderSymbol, INamedTypeSymbol orderItemSymbol, INamedTypeSymbol productSymbol) _testData;

    public PathValidatorEdgeCasesTests()
    {
        _testData = TestHelper.CreateTestCompilation();
    }

    #region Whitespace Handling

    [Fact]
    public void ValidatePath_WithLeadingWhitespace_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "  Address";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].PropertyName.Should().Be("Address");
    }

    [Fact]
    public void ValidatePath_WithTrailingWhitespace_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "Address  ";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails[0].PropertyName.Should().Be("Address");
    }

    [Fact]
    public void ValidatePath_WithWhitespaceAroundDot_ShouldTrimAndSucceed()
    {
        // Arrange
        var path = "Orders . OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(2);
        result.SegmentDetails[0].PropertyName.Should().Be("Orders");
        result.SegmentDetails[1].PropertyName.Should().Be("OrderItems");
    }

    #endregion

    #region Multiple Dots

    [Fact]
    public void ValidatePath_WithConsecutiveDots_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders..OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_WithTrailingDot_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders.";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_WithLeadingDot_ShouldReturnInvalid()
    {
        // Arrange
        var path = ".Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Special Characters

    [Fact]
    public void ValidatePath_WithSpecialCharacters_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders@OrderItems";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Location Handling

    [Fact]
    public void ValidatePath_WithLocation_ShouldPreserveLocation()
    {
        // Arrange
        var path = "Orders";
        var location = Location.None;

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation, location);

        // Assert
        result.Location.Should().Be(location);
    }

    [Fact]
    public void ValidatePath_WithoutLocation_ShouldHaveNullLocation()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.Location.Should().BeNull();
    }

    #endregion

    #region Very Long Paths

    [Fact]
    public void ValidatePath_WithVeryLongValidPath_ShouldReturnValid()
    {
        // Arrange
        var path = "Orders.OrderItems.Product.Category"; // 4 levels

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Segments.Should().HaveCount(4);
        result.SegmentDetails.Should().HaveCount(4);
    }

    #endregion


    #region Null Compilation/Symbol

    [Fact]
    public void ValidatePath_WithNullCompilation_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Compilation cannot be null.");
    }

    [Fact]
    public void ValidatePath_WithNullRootEntity_ShouldReturnInvalid()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, null!, _testData.compilation);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Root entity type cannot be null.");
    }

    [Fact]
    public void ValidatePath_WithBothNull_ShouldReturnInvalidForRootEntity()
    {
        // Arrange
        var path = "Orders";

        // Act
        var result = PathValidator.ValidatePath(path, null!, null!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        // Valida root entity primero (orden de validación)
        result.ErrorMessage.Should().Be("Root entity type cannot be null.");
    }

    #endregion

    #region SegmentInfo Property Access

    [Fact]
    public void ValidatePath_SegmentInfo_ShouldExposeContainingTypeAndPropertySymbol()
    {
        // Arrange - Address is a non-collection navigation on Customer
        var path = "Address";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        var segment = result.SegmentDetails[0];
        segment.ContainingType.Should().NotBeNull();
        segment.ContainingType.Name.Should().Be("Customer");
        segment.PropertySymbol.Should().NotBeNull();
        segment.PropertySymbol.Name.Should().Be("Address");
    }

    [Fact]
    public void ValidatePath_NestedSegmentInfo_ShouldExposeContainingTypeForEachLevel()
    {
        // Arrange
        var path = "Orders.OrderItems.Product";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeTrue();
        result.SegmentDetails.Should().HaveCount(3);

        result.SegmentDetails[0].ContainingType.Name.Should().Be("Customer");
        result.SegmentDetails[0].PropertySymbol.Name.Should().Be("Orders");

        result.SegmentDetails[1].ContainingType.Name.Should().Be("Order");
        result.SegmentDetails[1].PropertySymbol.Name.Should().Be("OrderItems");

        result.SegmentDetails[2].ContainingType.Name.Should().Be("OrderItem");
        result.SegmentDetails[2].PropertySymbol.Name.Should().Be("Product");
    }

    #endregion

    #region IsCollectionType - Non-INamedTypeSymbol

    [Fact]
    public void ValidatePath_WithScalarProperty_ShouldReturnInvalidNavigation()
    {
        // Arrange - "Name" is a string (scalar), not a navigation property
        var path = "Name";

        // Act
        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("is not a navigation property");
        result.ErrorMessage.Should().Contain("scalar value");
    }

    [Fact]
    public void ValidatePath_WithArrayProperty_ShouldReturnInvalidBecauseArrayIsNotSupportedCollection()
    {
        // Arrange - An array property (Order[]) is IArrayTypeSymbol, NOT INamedTypeSymbol
        // This exercises the "is not INamedTypeSymbol" branch in IsCollectionType
        var entityCode = @"
using System;
namespace TestDomain
{
    public class Entity
    {
        public Guid Id { get; set; }
        public Order[] Orders { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
    }
}";
        var (compilation, entitySymbol) = TestHelper.CreateSimpleCompilation(entityCode);

        // Act
        var result = PathValidator.ValidatePath("Orders", entitySymbol, compilation);

        // Assert
        // Array type (Order[]) is not INamedTypeSymbol, so IsCollectionType returns false
        // Then it falls through to IsScalarType check, which also fails for arrays
        // So it should be treated as invalid navigation (scalar)
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("is not a navigation property");
    }

    #endregion

    #region CalculateLevenshteinDistance Edge Cases

    [Fact]
    public void ValidatePath_WithEmptySegmentAfterValidPath_ShouldReturnInvalid()
    {
        var path = "Zz";

        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePath_TypoWithSingleChar_ShouldExerciseLevenshteinDistance()
    {
        var path = "I";

        var result = PathValidator.ValidatePath(path, _testData.customerSymbol, _testData.compilation);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Did you mean");
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithEmptySource_ShouldReturnTargetLength()
    {
        // Arrange - invoke private static method via reflection
        var method = typeof(PathValidator).GetMethod(
            "CalculateLevenshteinDistance",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = (int)method!.Invoke(null, new object[] { "", "hello" })!;

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithEmptyTarget_ShouldReturnSourceLength()
    {
        var method = typeof(PathValidator).GetMethod(
            "CalculateLevenshteinDistance",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = (int)method!.Invoke(null, new object[] { "hello", "" })!;

        result.Should().Be(5);
    }

    [Fact]
    public void CalculateLevenshteinDistance_WithBothEmpty_ShouldReturnZero()
    {
        var method = typeof(PathValidator).GetMethod(
            "CalculateLevenshteinDistance",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = (int)method!.Invoke(null, new object[] { "", "" })!;

        result.Should().Be(0);
    }

    #endregion
}

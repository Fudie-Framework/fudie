namespace Fudie.Validation.UnitTests;

public class ErrorCodeTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "{PropertyName} is required");

        errorCode.Aggregate.Should().Be("Customer");
        errorCode.Property.Should().Be("Name");
        errorCode.Rule.Should().Be("Required");
        errorCode.Message.Should().Be("{PropertyName} is required");
    }

    [Fact]
    public void Category_ShouldDefaultToFluentValidation()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "msg");

        errorCode.Category.Should().Be(ErrorCodeCategory.FluentValidation);
    }

    [Fact]
    public void Constructor_WithExplicitCategory_ShouldSetCategory()
    {
        var errorCode = new ErrorCode("Customer", "IsActive", "AlreadyActive", "msg", ErrorCodeCategory.Guard);

        errorCode.Category.Should().Be(ErrorCodeCategory.Guard);
    }

    [Fact]
    public void Code_ShouldReturnAggregateDotPropertyDotRule()
    {
        var errorCode = new ErrorCode("Customer", "Name", "Required", "msg");

        errorCode.Code.Should().Be("Customer.Name.Required");
    }

    [Fact]
    public void Code_ShouldReflectPropertyValues()
    {
        var errorCode = new ErrorCode("Order", "Total", "MinValue", "msg");

        errorCode.Code.Should().Be("Order.Total.MinValue");
    }

    [Fact]
    public void Record_ShouldSupportValueEquality()
    {
        var a = new ErrorCode("Customer", "Name", "Required", "msg");
        var b = new ErrorCode("Customer", "Name", "Required", "msg");

        a.Should().Be(b);
    }

    [Fact]
    public void Record_ShouldNotBeEqualWhenDifferent()
    {
        var a = new ErrorCode("Customer", "Name", "Required", "msg");
        var b = new ErrorCode("Customer", "Name", "MaxLength", "msg");

        a.Should().NotBe(b);
    }
}

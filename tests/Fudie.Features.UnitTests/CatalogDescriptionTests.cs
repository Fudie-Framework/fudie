namespace Fudie.Features.UnitTests;

public class CatalogDescriptionTests
{
    [Fact]
    public void CatalogDescription_ShouldStoreDescription()
    {
        var description = new CatalogDescription("Create a new menu");

        description.Description.Should().Be("Create a new menu");
    }

    [Fact]
    public void CatalogDescription_WithEmptyString_ShouldStoreEmpty()
    {
        var description = new CatalogDescription("");

        description.Description.Should().BeEmpty();
    }
}

namespace Fudie.Features.UnitTests;

public class CatalogEntryTests
{
    [Fact]
    public void CatalogEntry_ShouldStoreAllProperties()
    {
        var entry = new CatalogEntry(
            ClassName: "GetMenu",
            HttpVerb: "GET",
            RoutePattern: "/menus",
            IsAnonymous: false,
            IsAuthenticated: true,
            IsInternal: false,
            IsPlatform: false,
            Description: "Get all menus",
            AggregateId: "menu",
            AggregateDisplayName: "Menus",
            AggregateIcon: "book-open",
            AggregateReadDescription: "View menus",
            AggregateWriteDescription: "Manage menus",
            Scope: "menu:read",
            ScopeDescription: "View menus");

        entry.ClassName.Should().Be("GetMenu");
        entry.HttpVerb.Should().Be("GET");
        entry.RoutePattern.Should().Be("/menus");
        entry.IsAnonymous.Should().BeFalse();
        entry.IsAuthenticated.Should().BeTrue();
        entry.IsInternal.Should().BeFalse();
        entry.IsPlatform.Should().BeFalse();
        entry.Description.Should().Be("Get all menus");
        entry.AggregateId.Should().Be("menu");
        entry.AggregateDisplayName.Should().Be("Menus");
        entry.AggregateIcon.Should().Be("book-open");
        entry.AggregateReadDescription.Should().Be("View menus");
        entry.AggregateWriteDescription.Should().Be("Manage menus");
        entry.Scope.Should().Be("menu:read");
        entry.ScopeDescription.Should().Be("View menus");
    }

    [Fact]
    public void CatalogEntry_WithNullDescription_ShouldAllowNull()
    {
        var entry = new CatalogEntry(
            ClassName: "CreateMenu",
            HttpVerb: "POST",
            RoutePattern: "/menus",
            IsAnonymous: false,
            IsAuthenticated: false,
            IsInternal: false,
            IsPlatform: false,
            Description: null,
            AggregateId: "menu",
            AggregateDisplayName: "Menus",
            AggregateIcon: null,
            AggregateReadDescription: "View",
            AggregateWriteDescription: "Manage",
            Scope: "menu:write",
            ScopeDescription: "Manage menus");

        entry.Description.Should().BeNull();
        entry.AggregateIcon.Should().BeNull();
    }

    [Fact]
    public void CatalogEntry_ShouldSupportValueEquality()
    {
        var entry1 = new CatalogEntry("A", "GET", "/a", false, false, false, false, null,
            "agg", "Agg", null, "Read", "Write", "agg:read", "Read");
        var entry2 = new CatalogEntry("A", "GET", "/a", false, false, false, false, null,
            "agg", "Agg", null, "Read", "Write", "agg:read", "Read");

        entry1.Should().Be(entry2);
    }
}

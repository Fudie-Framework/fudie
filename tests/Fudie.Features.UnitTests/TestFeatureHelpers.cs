// Global namespace — Type.Namespace returns null, used to test ResolveAggregate null-namespace path
// Internal so MapFeatures (which filters IsPublic) does not discover it
internal class GlobalNamespaceFeatureModule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app) { }
}

namespace Fudie.Features.UnitTests.TestAggregate
{
    public class TestAggregateDescription : IAggregateDescription
    {
        public string Id => "test";
        public string DisplayName => "Test";
        public string? Icon => "test-icon";
        public string ReadDescription => "Read tests";
        public string WriteDescription => "Write tests";
    }
}

namespace Fudie.Features.UnitTests.TestAggregate.Commands
{
    public class TestFeatureModule : IFeatureModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-feature", () => "test");
        }
    }
}

// Global namespace — Type.Namespace returns null, used to test null-namespace path
// Internal so MapFeatures (which filters IsPublic) does not discover it
internal class GlobalNamespaceFeatureModule : IFeatureModule
{
    public void AddRoutes(IEndpointRouteBuilder app) { }
}

namespace Fudie.Http.UnitTests.TestAggregate.Commands
{
    public class TestFeatureModule : IFeatureModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-feature", () => "test");
        }
    }
}

namespace Fudie.Security.Http;

/// <summary>
/// Extension methods for mapping the catalog discovery endpoint.
/// </summary>
public static class CatalogEndpointExtensions
{
    /// <summary>
    /// Maps the <c>GET /catalog</c> endpoint that returns all registered catalog entries.
    /// </summary>
    public static void MapCatalog(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/catalog", (
            ICatalogRegistry catalog,
            IConfiguration configuration) =>
        {
            var response = new
            {
                ServiceId = configuration["Fudie:ServiceId"],
                ServiceName = configuration["Fudie:ServiceName"],
                Entries = catalog.GetAll()
            };

            return Results.Ok(response);
        })
        .RequireInternal();
    }
}

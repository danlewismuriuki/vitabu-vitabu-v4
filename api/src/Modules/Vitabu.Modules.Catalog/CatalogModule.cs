using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Modules.Catalog.Contracts;
using Vitabu.Modules.Catalog.Services;

namespace Vitabu.Modules.Catalog;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/titles", async (
            string? q,
            string? grade,
            string? subject,
            int? page,
            int? page_size,
            ICatalogReadService catalog,
            CancellationToken ct) =>
        {
            var query = new SearchCbcTitlesQuery(q, grade, subject, page ?? 1, page_size ?? 20);
            return Results.Ok(await catalog.SearchTitlesAsync(query, ct));
        })
        .WithName("searchCatalogTitles")
        .WithTags("Catalog");

        return app;
    }
}

public static class CatalogDependencyInjection
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<ICatalogReadService, CatalogReadService>();
        return services;
    }
}

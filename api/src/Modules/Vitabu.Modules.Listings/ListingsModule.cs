using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Services;

namespace Vitabu.Modules.Listings;

public static class ListingsEndpoints
{
    public static IEndpointRouteBuilder MapListingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/catalog/facets", async (IListingsReadService listings, CancellationToken ct) =>
            Results.Ok(await listings.GetFacetsAsync(ct)))
            .WithName("getCatalogFacets")
            .WithTags("Catalog");

        app.MapGet("/listings", async (
            string? q,
            string? grade,
            string? subject,
            string? city,
            string? intent,
            string? condition,
            int? page,
            int? page_size,
            IListingsReadService listings,
            CancellationToken ct) =>
        {
            var query = new ListListingsQuery(
                q,
                grade,
                subject,
                city,
                ParseEnum<ListingIntent>(intent),
                ParseEnum<BookCondition>(condition),
                page ?? 1,
                page_size ?? 20);

            return Results.Ok(await listings.ListAsync(query, ct));
        })
        .WithName("listListings")
        .WithTags("Listings");

        app.MapGet("/listings/{id:guid}", async (Guid id, IListingsReadService listings, CancellationToken ct) =>
            Results.Ok(await listings.GetAsync(id, ct)))
            .WithName("getListing")
            .WithTags("Listings");

        return app;
    }

    private static TEnum? ParseEnum<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace("-", "_");
        foreach (var name in Enum.GetNames<TEnum>())
        {
            var snake = ToSnake(name);
            if (snake.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<TEnum>(name);
            }
        }

        return null;
    }

    private static string ToSnake(string pascal)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c) && i > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}

public static class ListingsDependencyInjection
{
    public static IServiceCollection AddListingsModule(this IServiceCollection services)
    {
        services.AddScoped<IListingsReadService, ListingsReadService>();
        return services;
    }
}

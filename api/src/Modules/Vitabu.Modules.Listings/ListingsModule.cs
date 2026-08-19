using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Services;
using Vitabu.Modules.Listings.Validation;

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
            Guid? school_id,
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
                school_id,
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

        app.MapPost("/listings", async (
            CreateListingRequest request,
            ClaimsPrincipal principal,
            IValidator<CreateListingRequest> validator,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var userId = RequireUserId(principal);
            var created = await write.CreateAsync(userId, request, ct);
            return Results.Created($"/listings/{created.Id}", created);
        })
        .RequireAuthorization()
        .WithName("createListing")
        .WithTags("Listings");

        app.MapGet("/me/listings", async (
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await write.ListMineAsync(userId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listMyListings")
        .WithTags("Listings");

        app.MapGet("/me/listings/{id:guid}", async (
            Guid id,
            ClaimsPrincipal principal,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await write.GetMineAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("getMyListing")
        .WithTags("Listings");

        app.MapPatch("/listings/{id:guid}", async (
            Guid id,
            UpdateListingRequest request,
            ClaimsPrincipal principal,
            IValidator<UpdateListingRequest> validator,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var userId = RequireUserId(principal);
            return Results.Ok(await write.UpdateAsync(userId, id, request, ct));
        })
        .RequireAuthorization()
        .WithName("updateListing")
        .WithTags("Listings");

        app.MapPost("/listings/{id:guid}/pause", async (
            Guid id,
            ClaimsPrincipal principal,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await write.PauseAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("pauseListing")
        .WithTags("Listings");

        app.MapPost("/listings/{id:guid}/resume", async (
            Guid id,
            ClaimsPrincipal principal,
            IListingsWriteService write,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await write.ResumeAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("resumeListing")
        .WithTags("Listings");

        app.MapPost("/media/image-stub", (
            ImageStubRequest? request,
            IListingsWriteService write) =>
            Results.Ok(write.CreateImageStub(request ?? new ImageStubRequest(null))))
            .RequireAuthorization()
            .WithName("createImageStub")
            .WithTags("Listings");

        return app;
    }

    private static Guid RequireUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("sub");
        if (sub is null || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedDomainException("unauthorized", "Authentication required.");
        }

        return userId;
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(e => ToSnakeCase(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        throw new Vitabu.Core.Exceptions.ValidationException("One or more validation errors occurred.", errors);
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
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
            var snake = ToSnakeCase(name);
            if (snake.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<TEnum>(name);
            }
        }

        return null;
    }
}

public static class ListingsDependencyInjection
{
    public static IServiceCollection AddListingsModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateListingRequestValidator>();
        services.AddScoped<IListingsReadService, ListingsReadService>();
        services.AddScoped<IListingsWriteService, ListingsWriteService>();
        return services;
    }
}

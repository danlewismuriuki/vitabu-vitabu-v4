using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Deals.Contracts;
using Vitabu.Modules.Deals.PickupMtaani;
using Vitabu.Modules.Deals.Services;
using Vitabu.Modules.Deals.Validation;

namespace Vitabu.Modules.Deals;

public static class DealsEndpoints
{
    public static IEndpointRouteBuilder MapDealsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/mtaani/locations", async (
            string? search,
            IPickupMtaaniClient mtaani,
            CancellationToken ct) =>
        {
            var items = await mtaani.ListLocationsAsync(search, ct);
            return Results.Ok(items.Select(l => new MtaaniLocationCard(l.Id, l.Name, l.ZoneId)).ToList());
        })
        .WithName("listMtaaniLocations")
        .WithTags("PickupMtaani");

        app.MapGet("/mtaani/agents", async (
            int? location_id,
            string? search,
            IPickupMtaaniClient mtaani,
            CancellationToken ct) =>
        {
            var items = await mtaani.ListAgentsAsync(location_id, search, ct);
            return Results.Ok(items.Select(a => new MtaaniAgentCard(
                a.Id,
                a.BusinessName,
                a.LocationId,
                a.LocationName,
                a.Area)).ToList());
        })
        .WithName("listMtaaniAgents")
        .WithTags("PickupMtaani");

        app.MapGet("/mtaani/delivery-charge", async (
            int sender_agent_id,
            int receiver_agent_id,
            IPickupMtaaniClient mtaani,
            CancellationToken ct) =>
        {
            var charge = await mtaani.GetAgentPackageChargeAsync(sender_agent_id, receiver_agent_id, ct);
            if (charge is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new MtaaniDeliveryChargeCard(charge.AmountKes, charge.Currency ?? "KES"));
        })
        .WithName("getMtaaniDeliveryCharge")
        .WithTags("PickupMtaani");

        app.MapPost("/listings/{listingId:guid}/interests", async (
            Guid listingId,
            CreateInterestRequest request,
            ClaimsPrincipal principal,
            IValidator<CreateInterestRequest> validator,
            IDealsService deals,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var userId = RequireUserId(principal);
            var created = await deals.CreateAsync(userId, listingId, request, ct);
            return Results.Created($"/interests/{created.Id}", created);
        })
        .RequireAuthorization()
        .WithName("createInterest")
        .WithTags("Deals");

        app.MapGet("/me/interests", async (
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.ListMineAsBuyerAsync(userId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listMyInterests")
        .WithTags("Deals");

        app.MapGet("/me/listings/{listingId:guid}/interests", async (
            Guid listingId,
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.ListForListingAsSellerAsync(userId, listingId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listListingInterests")
        .WithTags("Deals");

        app.MapGet("/interests/{id:guid}", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.GetAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("getInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/accept", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.AcceptAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("acceptInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/decline", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.DeclineAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("declineInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/cancel", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.CancelAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("cancelInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/release", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.ReleaseAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("releaseInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/complete", async (
            Guid id,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.CompleteAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("completeInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/dispute", async (
            Guid id,
            DisputeInterestRequest request,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await deals.DisputeAsync(userId, id, request, ct));
        })
        .RequireAuthorization()
        .WithName("disputeInterest")
        .WithTags("Deals");

        app.MapPost("/interests/{id:guid}/rate", async (
            Guid id,
            RateInterestRequest request,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await deals.RateAsync(userId, id, request, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("rateInterest")
        .WithTags("Deals");

        app.MapPost("/listings/{listingId:guid}/reports", async (
            Guid listingId,
            ReportListingRequest request,
            ClaimsPrincipal principal,
            IDealsService deals,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await deals.ReportListingAsync(userId, listingId, request, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("reportListing")
        .WithTags("Deals");

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
}

public static class DealsDependencyInjection
{
    public static IServiceCollection AddDealsModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateInterestRequestValidator>();
        services.AddScoped<IDealsService, DealsService>();
        return services;
    }
}

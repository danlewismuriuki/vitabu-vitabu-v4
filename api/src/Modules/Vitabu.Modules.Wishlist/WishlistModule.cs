using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Wishlist.Services;

namespace Vitabu.Modules.Wishlist;

public static class WishlistEndpoints
{
    public static IEndpointRouteBuilder MapWishlistEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/wishlist", async (
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            IWishlistService wishlist,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await wishlist.ListMineAsync(userId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listMyWishlist")
        .WithTags("Wishlist");

        app.MapGet("/listings/{id:guid}/wishlist", async (
            Guid id,
            ClaimsPrincipal principal,
            IWishlistService wishlist,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await wishlist.GetStatusAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("getWishlistStatus")
        .WithTags("Wishlist");

        app.MapPost("/listings/{id:guid}/wishlist", async (
            Guid id,
            ClaimsPrincipal principal,
            IWishlistService wishlist,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await wishlist.AddAsync(userId, id, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("addToWishlist")
        .WithTags("Wishlist");

        app.MapDelete("/listings/{id:guid}/wishlist", async (
            Guid id,
            ClaimsPrincipal principal,
            IWishlistService wishlist,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await wishlist.RemoveAsync(userId, id, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("removeFromWishlist")
        .WithTags("Wishlist");

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
}

public static class WishlistDependencyInjection
{
    public static IServiceCollection AddWishlistModule(this IServiceCollection services)
    {
        services.AddScoped<IWishlistService, WishlistService>();
        return services;
    }
}

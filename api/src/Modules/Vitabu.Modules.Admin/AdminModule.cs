using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Admin.Contracts;
using Vitabu.Modules.Admin.Services;

namespace Vitabu.Modules.Admin;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin").WithTags("Admin").RequireAuthorization();

        group.MapGet("/listings", async (
            string? status,
            int? page,
            int? page_size,
            ClaimsPrincipal principal,
            IAdminService admin,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await admin.EnsureStaffAsync(userId, ct);
            return Results.Ok(await admin.ListListingsAsync(status, page ?? 1, page_size ?? 20, ct));
        }).WithName("adminListListings");

        group.MapPost("/listings/{id:guid}/hide", async (
            Guid id,
            ClaimsPrincipal principal,
            IAdminService admin,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await admin.HideListingAsync(userId, id, ct);
            return Results.NoContent();
        }).WithName("adminHideListing");

        group.MapGet("/reports", async (
            string? status,
            int? page,
            int? page_size,
            ClaimsPrincipal principal,
            IAdminService admin,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await admin.EnsureStaffAsync(userId, ct);
            return Results.Ok(await admin.ListReportsAsync(status, page ?? 1, page_size ?? 20, ct));
        }).WithName("adminListReports");

        group.MapPost("/reports/{id:guid}/resolve", async (
            Guid id,
            ResolveReportRequest request,
            ClaimsPrincipal principal,
            IAdminService admin,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await admin.ResolveReportAsync(userId, id, request, ct);
            return Results.NoContent();
        }).WithName("adminResolveReport");

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

public static class AdminDependencyInjection
{
    public static IServiceCollection AddAdminModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Notifications.Services;

namespace Vitabu.Modules.Notifications;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/notifications", async (
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            INotificationService notifications,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await notifications.ListMineAsync(userId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listMyNotifications")
        .WithTags("Notifications");

        app.MapPost("/me/notifications/{id:guid}/read", async (
            Guid id,
            ClaimsPrincipal principal,
            INotificationService notifications,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            await notifications.MarkReadAsync(userId, id, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("markNotificationRead")
        .WithTags("Notifications");

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

public static class NotificationsDependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}

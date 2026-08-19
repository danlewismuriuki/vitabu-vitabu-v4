using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Messaging.Contracts;
using Vitabu.Modules.Messaging.Services;
using Vitabu.Modules.Messaging.Validation;

namespace Vitabu.Modules.Messaging;

public static class MessagingEndpoints
{
    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/listings/{id:guid}/threads", async (
            Guid id,
            ClaimsPrincipal principal,
            IMessagingService messaging,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            var thread = await messaging.OpenThreadAsync(userId, id, ct);
            return Results.Ok(thread);
        })
        .RequireAuthorization()
        .WithName("openListingThread")
        .WithTags("Messaging");

        app.MapGet("/me/threads", async (
            ClaimsPrincipal principal,
            int? page,
            int? page_size,
            IMessagingService messaging,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await messaging.ListMineAsync(userId, page ?? 1, page_size ?? 20, ct));
        })
        .RequireAuthorization()
        .WithName("listMyThreads")
        .WithTags("Messaging");

        app.MapGet("/threads/{id:guid}", async (
            Guid id,
            ClaimsPrincipal principal,
            IMessagingService messaging,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            return Results.Ok(await messaging.GetThreadAsync(userId, id, ct));
        })
        .RequireAuthorization()
        .WithName("getThread")
        .WithTags("Messaging");

        app.MapPost("/threads/{id:guid}/messages", async (
            Guid id,
            SendMessageRequest body,
            ClaimsPrincipal principal,
            IMessagingService messaging,
            CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            var message = await messaging.SendAsync(userId, id, body, ct);
            return Results.Ok(message);
        })
        .RequireAuthorization()
        .WithName("sendThreadMessage")
        .WithTags("Messaging");

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

public static class MessagingDependencyInjection
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services)
    {
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddValidatorsFromAssemblyContaining<SendMessageRequestValidator>();
        return services;
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Identity.Contracts;
using Vitabu.Modules.Identity.Services;

namespace Vitabu.Modules.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IValidator<RegisterRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var result = await identity.RegisterAsync(request, ct);
            return Results.Created("/auth/me", result);
        }).WithName("register");

        group.MapPost("/login", async (
            LoginRequest request,
            IValidator<LoginRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var result = await identity.LoginAsync(request, ct);
            return Results.Ok(result);
        }).WithName("login");

        group.MapGet("/me", async (ClaimsPrincipal principal, IIdentityService identity, CancellationToken ct) =>
        {
            var userId = RequireUserId(principal);
            var profile = await identity.GetMeAsync(userId, ct);
            return Results.Ok(profile);
        }).RequireAuthorization().WithName("getMe");

        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            IValidator<ForgotPasswordRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var result = await identity.ForgotPasswordAsync(request, ct);
            return Results.Accepted((string?)null, result);
        }).WithName("forgotPassword");

        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            IValidator<ResetPasswordRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var result = await identity.ResetPasswordAsync(request, ct);
            return Results.Ok(result);
        }).WithName("resetPassword");

        group.MapPost("/phone/request-otp", async (
            RequestPhoneOtpRequest request,
            ClaimsPrincipal principal,
            IValidator<RequestPhoneOtpRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var userId = RequireUserId(principal);
            var result = await identity.RequestPhoneOtpAsync(userId, request, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("requestPhoneOtp");

        group.MapPost("/phone/verify-otp", async (
            VerifyPhoneOtpRequest request,
            ClaimsPrincipal principal,
            IValidator<VerifyPhoneOtpRequest> validator,
            IIdentityService identity,
            CancellationToken ct) =>
        {
            await ValidateAsync(validator, request, ct);
            var userId = RequireUserId(principal);
            var result = await identity.VerifyPhoneOtpAsync(userId, request, ct);
            return Results.Ok(result);
        }).RequireAuthorization().WithName("verifyPhoneOtp");

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

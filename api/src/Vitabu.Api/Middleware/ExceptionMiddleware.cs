using System.Net;
using System.Text.Json;
using Vitabu.Core.Exceptions;

namespace Vitabu.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, error, message, errors) = exception switch
        {
            ValidationException vex => (
                (int)HttpStatusCode.BadRequest,
                "validation_failed",
                vex.Message,
                vex.Errors),
            NotFoundException nf => (
                (int)HttpStatusCode.NotFound,
                nf.ErrorCode,
                nf.Message,
                (IDictionary<string, string[]>?)null),
            ConflictException cx => (
                (int)HttpStatusCode.Conflict,
                cx.ErrorCode,
                cx.Message,
                (IDictionary<string, string[]>?)null),
            DomainException dx => (
                (int)HttpStatusCode.BadRequest,
                dx.ErrorCode,
                dx.Message,
                (IDictionary<string, string[]>?)null),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "internal_error",
                "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null)
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogWarning(exception, "Request failed with {Error}", error);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var problem = new
        {
            error,
            message,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}

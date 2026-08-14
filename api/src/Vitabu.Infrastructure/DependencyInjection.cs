using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vitabu.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence and external adapters. S0 is a no-op placeholder;
    /// S1+ adds EF Core DbContext, MinIO, SMS/email.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;
        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Infrastructure.Persistence;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Deals.Persistence;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Persistence;

namespace Vitabu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");

        services.AddDbContext<VitabuDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());
        services.AddScoped<ICatalogDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());
        services.AddScoped<IListingsDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());
        services.AddScoped<IDealsDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());
        services.AddScoped<INotificationsDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());

        return services;
    }
}

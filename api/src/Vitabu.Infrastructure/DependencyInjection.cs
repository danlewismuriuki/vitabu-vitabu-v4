using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vitabu.Infrastructure.Persistence;
using Vitabu.Infrastructure.PickupMtaani;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Deals.Persistence;
using Vitabu.Modules.Deals.PickupMtaani;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Persistence;
using Vitabu.Modules.Wishlist.Persistence;
using Vitabu.Modules.Messaging.Persistence;

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
        services.AddScoped<IWishlistDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());
        services.AddScoped<IMessagingDbContext>(sp => sp.GetRequiredService<VitabuDbContext>());

        services.Configure<PickupMtaaniOptions>(configuration.GetSection(PickupMtaaniOptions.SectionName));
        var mtaani = configuration.GetSection(PickupMtaaniOptions.SectionName).Get<PickupMtaaniOptions>()
                     ?? new PickupMtaaniOptions();
        if (string.IsNullOrWhiteSpace(mtaani.ApiKey))
        {
            services.AddSingleton<IPickupMtaaniClient, DevPickupMtaaniClient>();
        }
        else
        {
            services.AddHttpClient<IPickupMtaaniClient, HttpPickupMtaaniClient>(client =>
            {
                client.BaseAddress = new Uri(mtaani.BaseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Add("apiKey", mtaani.ApiKey);
            });
        }

        return services;
    }
}

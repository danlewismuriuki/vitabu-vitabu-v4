using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Vitabu.Api.Middleware;
using Vitabu.Infrastructure;
using Vitabu.Infrastructure.Persistence;
using Vitabu.Infrastructure.Seed;
using Vitabu.Modules.Admin;
using Vitabu.Modules.Catalog;
using Vitabu.Modules.Deals;
using Vitabu.Modules.Identity;
using Vitabu.Modules.Listings;
using Vitabu.Modules.Notifications;
using Vitabu.Modules.Wishlist;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule();
builder.Services.AddListingsModule();
builder.Services.AddNotificationsModule();
builder.Services.AddDealsModule();
builder.Services.AddAdminModule();
builder.Services.AddWishlistModule();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3002",
                "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VitabuDbContext>();
    await db.Database.MigrateAsync();
}

await CatalogSeed.SeedAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "vitabu-api",
    utc = DateTime.UtcNow
}))
.WithName("getHealth");

app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapListingsEndpoints();
app.MapDealsEndpoints();
app.MapNotificationsEndpoints();
app.MapAdminEndpoints();
app.MapWishlistEndpoints();

app.Run();

public partial class Program;

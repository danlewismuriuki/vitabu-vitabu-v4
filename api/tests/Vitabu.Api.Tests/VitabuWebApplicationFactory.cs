using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Vitabu.Api.Tests;

public sealed class VitabuWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connection = Environment.GetEnvironmentVariable("VITABU_TEST_POSTGRES")
            ?? "Host=localhost;Port=5433;Database=vitabu;Username=vitabu;Password=vitabu";

        builder.UseSetting("ConnectionStrings:Postgres", connection);
        builder.UseSetting("Jwt:Key", "dev-only-vitabu-jwt-signing-key-min-32-chars!");
        builder.UseSetting("Jwt:Issuer", "vitabu-api");
        builder.UseSetting("Jwt:Audience", "vitabu-web");
        builder.UseSetting("Smtp:Host", "localhost");
        builder.UseSetting("Smtp:Port", "1025");
    }
}

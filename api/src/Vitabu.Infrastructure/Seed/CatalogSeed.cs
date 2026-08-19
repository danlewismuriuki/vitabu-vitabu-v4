using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vitabu.Modules.Catalog.Entities;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Entities;
using Vitabu.Infrastructure.Persistence;

namespace Vitabu.Infrastructure.Seed;

public static class CatalogSeed
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VitabuDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CatalogSeed");
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var path = ResolveCatalogPath(config, env);
        if (!File.Exists(path))
        {
            logger.LogWarning("CBC catalog seed file not found at {Path}", path);
            return;
        }

        await using var stream = File.OpenRead(path);
        var doc = await JsonSerializer.DeserializeAsync<CatalogFile>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }, ct);

        if (doc?.Titles is null || doc.Titles.Count == 0)
        {
            logger.LogWarning("CBC catalog seed file has no titles");
            return;
        }

        if (!await db.CbcTitles.AnyAsync(ct))
        {
            foreach (var t in doc.Titles)
            {
                db.CbcTitles.Add(new CbcTitle
                {
                    Id = Guid.NewGuid(),
                    Code = t.Code,
                    Title = t.Title,
                    Grade = t.Grade,
                    Subject = t.Subject,
                    Term = t.Term,
                    MaterialType = t.MaterialType,
                    Language = t.Language
                });
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} CBC titles", doc.Titles.Count);
        }

        var seller = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "SEED@VITABU.LOCAL", ct);
        if (seller is null)
        {
            var now = DateTime.UtcNow;
            seller = new User
            {
                Id = Guid.NewGuid(),
                Email = "seed@vitabu.local",
                NormalizedEmail = "SEED@VITABU.LOCAL",
                DisplayName = "Vitabu Seed Parent",
                City = doc.Cities?.FirstOrDefault() ?? "Nairobi",
                AcceptedTermsAtUtc = now,
                ConfirmedParentGuardian = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            seller.PasswordHash = hasher.HashPassword(seller, "SeedPassword1!");
            db.Users.Add(seller);
            await db.SaveChangesAsync(ct);
        }

        if (await db.Listings.AnyAsync(ct))
        {
            await EnsureAdminAsync(db, hasher, config, logger, ct);
            await EnsureSchoolsAsync(db, logger, ct);
            return;
        }

        var titles = await db.CbcTitles.AsNoTracking().ToListAsync(ct);
        var cities = doc.Cities is { Count: > 0 } ? doc.Cities : ["Nairobi", "Mombasa", "Kisumu"];
        var intents = new[]
        {
            ListingIntent.Sale,
            ListingIntent.Free,
            ListingIntent.Exchange,
            ListingIntent.DonateSchool
        };
        var conditions = new[] { BookCondition.LikeNew, BookCondition.Good, BookCondition.Fair, BookCondition.WritingInside };
        var nowUtc = DateTime.UtcNow;

        for (var i = 0; i < titles.Count; i++)
        {
            var title = titles[i];
            var intent = intents[i % intents.Length];
            var condition = conditions[i % conditions.Length];
            var city = cities[i % cities.Count];
            var id = Guid.NewGuid();
            var slug = $"{Slugify(title.Title)}-{id.ToString("N")[..8]}";

            db.Listings.Add(new Listing
            {
                Id = id,
                SellerUserId = seller.Id,
                CbcTitleId = title.Id,
                Title = title.Title,
                Grade = title.Grade,
                Subject = title.Subject,
                Term = title.Term,
                City = city,
                Intent = intent,
                Condition = condition,
                Status = ListingStatus.Active,
                PriceKes = intent == ListingIntent.Sale ? 300 + (i * 50) : null,
                Description =
                    $"{title.Title} in {condition} condition. Listed for {intent} from {city}. CBC {title.Grade} / {title.Subject}.",
                Slug = slug,
                InterestCount = i % 4,
                CreatedAtUtc = nowUtc.AddMinutes(-i * 17),
                UpdatedAtUtc = nowUtc.AddMinutes(-i * 17)
            });
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} demo listings", titles.Count);
        await EnsureAdminAsync(db, hasher, config, logger, ct);
        await EnsureSchoolsAsync(db, logger, ct);
    }

    private static async Task EnsureSchoolsAsync(VitabuDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.Schools.AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var schools = new[]
        {
            new School
            {
                Id = Guid.NewGuid(),
                Name = "Olympic Primary School",
                City = "Nairobi",
                ContactName = "Head teacher",
                IsVerified = true,
                Notes = "Kibera — open to book donations.",
                CreatedAtUtc = now
            },
            new School
            {
                Id = Guid.NewGuid(),
                Name = "Kisumu Union Primary",
                City = "Kisumu",
                ContactName = "Donation desk",
                IsVerified = true,
                CreatedAtUtc = now
            },
            new School
            {
                Id = Guid.NewGuid(),
                Name = "Tononoka Primary School",
                City = "Mombasa",
                ContactName = "Deputy head",
                IsVerified = true,
                CreatedAtUtc = now
            }
        };

        db.Schools.AddRange(schools);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} schools", schools.Length);
    }

    private static async Task EnsureAdminAsync(
        VitabuDbContext db,
        IPasswordHasher<User> hasher,
        IConfiguration config,
        ILogger logger,
        CancellationToken ct)
    {
        var email = config["Seed:AdminEmail"] ?? "admin@vitabu.local";
        var password = config["Seed:AdminPassword"] ?? "AdminPassword1!";
        var normalized = email.Trim().ToUpperInvariant();
        var admin = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (admin is null)
        {
            var now = DateTime.UtcNow;
            admin = new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim(),
                NormalizedEmail = normalized,
                DisplayName = "Vitabu Staff",
                City = "Nairobi",
                IsStaff = true,
                PhoneVerifiedAtUtc = now,
                PhoneE164 = "+254700000001",
                AcceptedTermsAtUtc = now,
                ConfirmedParentGuardian = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            admin.PasswordHash = hasher.HashPassword(admin, password);
            db.Users.Add(admin);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded staff user {Email}", email);
            return;
        }

        if (!admin.IsStaff)
        {
            admin.IsStaff = true;
            admin.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Promoted {Email} to staff", email);
        }
    }

    private static string ResolveCatalogPath(IConfiguration config, IHostEnvironment env)
    {
        var configured = config["Seed:CatalogPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var rooted = Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
            if (File.Exists(rooted))
            {
                return rooted;
            }
        }

        var candidates = new[]
        {
            Path.Combine(env.ContentRootPath, "Data", "cbc-book-catalog.json"),
            Path.Combine(AppContext.BaseDirectory, "Data", "cbc-book-catalog.json"),
            Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "..", "data", "cbc-book-catalog.json")),
            Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "..", "..", "data", "cbc-book-catalog.json"))
        };

        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static string Slugify(string value)
    {
        var chars = value.ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private sealed class CatalogFile
    {
        public List<TitleFile> Titles { get; set; } = [];
        public List<string>? Cities { get; set; }
    }

    private sealed class TitleFile
    {
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public string Grade { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Term { get; set; } = "";
        public string MaterialType { get; set; } = "";
        public string Language { get; set; } = "";
    }
}

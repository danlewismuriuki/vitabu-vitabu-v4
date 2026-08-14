using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Catalog.Entities;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Entities;
using Vitabu.Modules.Listings.Persistence;

namespace Vitabu.Infrastructure.Persistence;

public sealed class VitabuDbContext(DbContextOptions<VitabuDbContext> options)
    : DbContext(options), IIdentityDbContext, ICatalogDbContext, IListingsDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneOtpChallenge> PhoneOtpChallenges => Set<PhoneOtpChallenge>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<CbcTitle> CbcTitles => Set<CbcTitle>();
    public DbSet<Listing> Listings => Set<Listing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CbcTitleConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

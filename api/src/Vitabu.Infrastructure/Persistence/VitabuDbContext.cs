using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Catalog.Entities;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Deals.Entities;
using Vitabu.Modules.Deals.Persistence;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Entities;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Entities;
using Vitabu.Modules.Notifications.Persistence;
using Vitabu.Modules.Wishlist.Entities;
using Vitabu.Modules.Wishlist.Persistence;

namespace Vitabu.Infrastructure.Persistence;

public sealed class VitabuDbContext(DbContextOptions<VitabuDbContext> options)
    : DbContext(options), IIdentityDbContext, ICatalogDbContext, IListingsDbContext, IDealsDbContext,
        INotificationsDbContext, IWishlistDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneOtpChallenge> PhoneOtpChallenges => Set<PhoneOtpChallenge>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<CbcTitle> CbcTitles => Set<CbcTitle>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<DealInterest> DealInterests => Set<DealInterest>();
    public DbSet<DealRating> DealRatings => Set<DealRating>();
    public DbSet<ListingReport> ListingReports => Set<ListingReport>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<WishlistEntry> WishlistEntries => Set<WishlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CbcTitleConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DealInterestConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppNotificationConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WishlistEntryConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

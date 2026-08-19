using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Wishlist.Entities;

namespace Vitabu.Modules.Wishlist.Persistence;

public interface IWishlistDbContext
{
    DbSet<WishlistEntry> WishlistEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class WishlistEntryConfiguration : IEntityTypeConfiguration<WishlistEntry>
{
    public void Configure(EntityTypeBuilder<WishlistEntry> builder)
    {
        builder.ToTable("wishlist_entries");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
    }
}

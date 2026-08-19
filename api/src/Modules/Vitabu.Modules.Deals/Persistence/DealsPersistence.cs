using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Deals.Entities;

namespace Vitabu.Modules.Deals.Persistence;

public interface IDealsDbContext
{
    DbSet<DealInterest> DealInterests { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class DealInterestConfiguration : IEntityTypeConfiguration<DealInterest>
{
    public void Configure(EntityTypeBuilder<DealInterest> builder)
    {
        builder.ToTable("deal_interests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.HandoffMode).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.City).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ListingId, x.BuyerUserId });
        builder.HasIndex(x => new { x.SellerUserId, x.Status });
        builder.HasIndex(x => new { x.BuyerUserId, x.Status });
        builder.HasIndex(x => x.ListingId);
    }
}

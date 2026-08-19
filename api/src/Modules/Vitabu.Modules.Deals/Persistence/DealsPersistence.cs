using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Deals.Entities;

namespace Vitabu.Modules.Deals.Persistence;

public interface IDealsDbContext
{
    DbSet<DealInterest> DealInterests { get; }
    DbSet<DealRating> DealRatings { get; }
    DbSet<ListingReport> ListingReports { get; }
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
        builder.Property(x => x.DisputeReason).HasMaxLength(500);
        builder.Property(x => x.MtaaniAgentName).HasMaxLength(200);
        builder.Property(x => x.MtaaniLocationName).HasMaxLength(200);
        builder.HasIndex(x => new { x.ListingId, x.BuyerUserId });
        builder.HasIndex(x => new { x.SellerUserId, x.Status });
        builder.HasIndex(x => new { x.BuyerUserId, x.Status });
        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.MtaaniAgentId);
    }
}

public sealed class DealRatingConfiguration : IEntityTypeConfiguration<DealRating>
{
    public void Configure(EntityTypeBuilder<DealRating> builder)
    {
        builder.ToTable("deal_ratings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => new { x.InterestId, x.FromUserId }).IsUnique();
        builder.HasIndex(x => x.ToUserId);
    }
}

public sealed class ListingReportConfiguration : IEntityTypeConfiguration<ListingReport>
{
    public void Configure(EntityTypeBuilder<ListingReport> builder)
    {
        builder.ToTable("listing_reports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        builder.HasIndex(x => x.ListingId);
    }
}

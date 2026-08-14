using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Listings.Entities;

namespace Vitabu.Modules.Listings.Persistence;

public interface IListingsDbContext
{
    DbSet<Listing> Listings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("listings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Grade).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Term).HasMaxLength(40);
        builder.Property(x => x.City).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Intent).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Condition).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PriceKes).HasPrecision(12, 2);
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.Slug).HasMaxLength(240).IsRequired();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.City, x.Grade, x.Subject });
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}

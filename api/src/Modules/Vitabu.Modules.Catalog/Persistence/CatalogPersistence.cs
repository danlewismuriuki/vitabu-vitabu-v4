using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Catalog.Entities;

namespace Vitabu.Modules.Catalog.Persistence;

public interface ICatalogDbContext
{
    DbSet<CbcTitle> CbcTitles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class CbcTitleConfiguration : IEntityTypeConfiguration<CbcTitle>
{
    public void Configure(EntityTypeBuilder<CbcTitle> builder)
    {
        builder.ToTable("cbc_titles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Grade).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Term).HasMaxLength(40).IsRequired();
        builder.Property(x => x.MaterialType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.Grade, x.Subject });
    }
}

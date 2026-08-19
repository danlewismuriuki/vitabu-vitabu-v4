using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Notifications.Entities;

namespace Vitabu.Modules.Notifications.Persistence;

public interface INotificationsDbContext
{
    DbSet<AppNotification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.UserId, x.ReadAtUtc });
    }
}

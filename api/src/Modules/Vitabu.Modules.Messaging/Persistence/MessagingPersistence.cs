using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Messaging.Entities;

namespace Vitabu.Modules.Messaging.Persistence;

public interface IMessagingDbContext
{
    DbSet<MessageThread> MessageThreads { get; }
    DbSet<Message> Messages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class MessageThreadConfiguration : IEntityTypeConfiguration<MessageThread>
{
    public void Configure(EntityTypeBuilder<MessageThread> builder)
    {
        builder.ToTable("message_threads");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ListingId, x.BuyerUserId }).IsUnique();
        builder.HasIndex(x => new { x.BuyerUserId, x.LastMessageAtUtc });
        builder.HasIndex(x => new { x.SellerUserId, x.LastMessageAtUtc });
    }
}

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Body).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.ThreadId, x.CreatedAtUtc });
    }
}

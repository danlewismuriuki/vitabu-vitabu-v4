using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Identity.Services;
using Vitabu.Modules.Notifications.Contracts;
using Vitabu.Modules.Notifications.Entities;
using Vitabu.Modules.Notifications.Persistence;

namespace Vitabu.Modules.Notifications.Services;

public interface INotificationService
{
    Task NotifyAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? relatedEntityId = null,
        string? emailSubject = null,
        string? emailBody = null,
        CancellationToken ct = default);

    Task<NotificationPage> ListMineAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default);
}

public sealed class NotificationService(
    INotificationsDbContext notificationsDb,
    IIdentityDbContext identityDb,
    IEmailSender email) : INotificationService
{
    public async Task NotifyAsync(
        Guid userId,
        string type,
        string title,
        string body,
        Guid? relatedEntityId = null,
        string? emailSubject = null,
        string? emailBody = null,
        CancellationToken ct = default)
    {
        notificationsDb.Notifications.Add(new AppNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            RelatedEntityId = relatedEntityId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await notificationsDb.SaveChangesAsync(ct);

        if (emailSubject is null)
        {
            return;
        }

        var user = await identityDb.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        await email.SendAsync(user.Email, emailSubject, emailBody ?? body, ct);
    }

    public async Task<NotificationPage> ListMineAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = notificationsDb.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId);

        var total = await q.CountAsync(ct);
        var unread = await q.CountAsync(n => n.ReadAtUtc == null, ct);
        var items = await q
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationItem(
                n.Id,
                n.Type,
                n.Title,
                n.Body,
                n.RelatedEntityId,
                n.CreatedAtUtc,
                n.ReadAtUtc))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new NotificationPage(items, page, pageSize, total, totalPages, unread);
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken ct = default)
    {
        var item = await notificationsDb.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);
        if (item is null || item.ReadAtUtc is not null)
        {
            return;
        }

        item.ReadAtUtc = DateTime.UtcNow;
        await notificationsDb.SaveChangesAsync(ct);
    }
}

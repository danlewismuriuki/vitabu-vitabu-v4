namespace Vitabu.Modules.Notifications.Contracts;

public sealed record NotificationItem(
    Guid Id,
    string Type,
    string Title,
    string Body,
    Guid? RelatedEntityId,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);

public sealed record NotificationPage(
    IReadOnlyList<NotificationItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    int UnreadCount);

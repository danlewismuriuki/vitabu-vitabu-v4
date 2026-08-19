namespace Vitabu.Modules.Messaging.Contracts;

public sealed record SendMessageRequest(string Body);

public sealed record MessageItem(
    Guid Id,
    Guid SenderUserId,
    string SenderDisplayName,
    string Body,
    DateTime CreatedAtUtc);

public sealed record ThreadCard(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid BuyerUserId,
    Guid SellerUserId,
    string OtherPartyName,
    string? LastMessagePreview,
    DateTime LastMessageAtUtc,
    DateTime CreatedAtUtc);

public sealed record ThreadDetail(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid BuyerUserId,
    Guid SellerUserId,
    string OtherPartyName,
    DateTime CreatedAtUtc,
    DateTime LastMessageAtUtc,
    IReadOnlyList<MessageItem> Messages);

public sealed record ThreadPage(
    IReadOnlyList<ThreadCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

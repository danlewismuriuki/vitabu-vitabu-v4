namespace Vitabu.Modules.Messaging.Entities;

public sealed class MessageThread
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerUserId { get; set; }
    public Guid SellerUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastMessageAtUtc { get; set; }
}

public sealed class Message
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

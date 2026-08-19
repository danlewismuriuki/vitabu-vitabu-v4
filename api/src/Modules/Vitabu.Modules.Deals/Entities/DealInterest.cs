using Vitabu.Modules.Deals.Domain;

namespace Vitabu.Modules.Deals.Entities;

public sealed class DealInterest
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerUserId { get; set; }
    public Guid SellerUserId { get; set; }
    public InterestStatus Status { get; set; }
    public HandoffMode HandoffMode { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? ReservedUntilUtc { get; set; }
}

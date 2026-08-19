using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Listings.Entities;

public sealed class Listing
{
    public Guid Id { get; set; }
    public Guid SellerUserId { get; set; }
    public Guid? CbcTitleId { get; set; }
    public Guid? SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Term { get; set; }
    public string City { get; set; } = string.Empty;
    public ListingIntent Intent { get; set; }
    public BookCondition Condition { get; set; }
    public ListingStatus Status { get; set; }
    public decimal? PriceKes { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int InterestCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

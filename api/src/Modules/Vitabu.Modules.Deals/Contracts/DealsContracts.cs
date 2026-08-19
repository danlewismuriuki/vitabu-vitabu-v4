using Vitabu.Modules.Deals.Domain;
using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Deals.Contracts;

public sealed record CreateInterestRequest(
    HandoffMode HandoffMode,
    string City,
    string? Message,
    int? MtaaniAgentId = null);

public sealed record PartySnippet(
    Guid Id,
    string DisplayName,
    string City,
    string? PhoneE164);

public sealed record MtaaniAgentSnippet(
    int Id,
    string BusinessName,
    int? LocationId,
    string? LocationName,
    int? EstimatedFeeKes);

public sealed record MtaaniLocationCard(int Id, string Name, int? ZoneId);

public sealed record MtaaniAgentCard(
    int Id,
    string BusinessName,
    int? LocationId,
    string? LocationName,
    string? Area);

public sealed record MtaaniDeliveryChargeCard(int AmountKes, string Currency);

public sealed record InterestDetail(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    ListingIntent ListingIntent,
    InterestStatus Status,
    HandoffMode HandoffMode,
    string City,
    string? Message,
    DateTime CreatedAtUtc,
    DateTime? AcceptedAtUtc,
    DateTime? ReservedUntilUtc,
    DateTime? BuyerCompletedAtUtc,
    DateTime? SellerCompletedAtUtc,
    string? DisputeReason,
    PartySnippet Buyer,
    PartySnippet Seller,
    MtaaniAgentSnippet? MtaaniAgent = null);

public sealed record InterestCard(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    InterestStatus Status,
    HandoffMode HandoffMode,
    string City,
    string BuyerDisplayName,
    DateTime CreatedAtUtc,
    DateTime? ReservedUntilUtc,
    string? MtaaniAgentName = null);

public sealed record InterestPage(
    IReadOnlyList<InterestCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record DisputeInterestRequest(string Reason);

public sealed record RateInterestRequest(int Stars, string? Comment);

public sealed record ReportListingRequest(string Reason, string? Details);

public sealed record ListingReportItem(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid ReporterUserId,
    string Reason,
    string? Details,
    string Status,
    DateTime CreatedAtUtc);

public sealed record ListingReportPage(
    IReadOnlyList<ListingReportItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

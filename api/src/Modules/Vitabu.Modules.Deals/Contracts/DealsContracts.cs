using Vitabu.Modules.Deals.Domain;
using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Deals.Contracts;

public sealed record CreateInterestRequest(
    HandoffMode HandoffMode,
    string City,
    string? Message);

public sealed record PartySnippet(
    Guid Id,
    string DisplayName,
    string City,
    string? PhoneE164);

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
    PartySnippet Buyer,
    PartySnippet Seller);

public sealed record InterestCard(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    InterestStatus Status,
    HandoffMode HandoffMode,
    string City,
    string BuyerDisplayName,
    DateTime CreatedAtUtc,
    DateTime? ReservedUntilUtc);

public sealed record InterestPage(
    IReadOnlyList<InterestCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

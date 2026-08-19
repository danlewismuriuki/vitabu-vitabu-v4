using Vitabu.Modules.Deals.Contracts;
using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Admin.Contracts;

public sealed record AdminListingCard(
    Guid Id,
    string Title,
    string City,
    ListingStatus Status,
    ListingIntent Intent,
    Guid SellerUserId,
    DateTime CreatedAtUtc,
    int InterestCount);

public sealed record AdminListingPage(
    IReadOnlyList<AdminListingCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ResolveReportRequest(string Action);

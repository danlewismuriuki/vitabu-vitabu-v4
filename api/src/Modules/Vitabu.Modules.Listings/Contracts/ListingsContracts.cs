using Vitabu.Modules.Listings.Domain;

namespace Vitabu.Modules.Listings.Contracts;

public sealed record CatalogFacets(
    IReadOnlyList<string> Grades,
    IReadOnlyList<string> Subjects,
    IReadOnlyList<string> Cities,
    IReadOnlyList<string> Intents,
    IReadOnlyList<string> Conditions);

public sealed record SellerSnippet(string DisplayName, string City);

public sealed record ListingCard(
    Guid Id,
    string Title,
    string Grade,
    string Subject,
    string? Term,
    string City,
    ListingIntent Intent,
    BookCondition Condition,
    ListingStatus Status,
    decimal? PriceKes,
    string? CoverImageUrl,
    int InterestCount,
    DateTime CreatedAtUtc);

public sealed record ListingDetail(
    Guid Id,
    string Title,
    string Grade,
    string Subject,
    string? Term,
    string City,
    ListingIntent Intent,
    BookCondition Condition,
    ListingStatus Status,
    decimal? PriceKes,
    string? CoverImageUrl,
    int InterestCount,
    DateTime CreatedAtUtc,
    string Description,
    string Slug,
    SellerSnippet Seller);

public sealed record ListingPage(
    IReadOnlyList<ListingCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ListListingsQuery(
    string? Q,
    string? Grade,
    string? Subject,
    string? City,
    ListingIntent? Intent,
    BookCondition? Condition,
    int Page = 1,
    int PageSize = 20);

public sealed record CreateListingRequest(
    Guid? CbcTitleId,
    string Title,
    string Grade,
    string Subject,
    string? Term,
    string City,
    ListingIntent Intent,
    BookCondition Condition,
    decimal? PriceKes,
    string Description,
    string CoverImageUrl);

public sealed record UpdateListingRequest(
    Guid? CbcTitleId,
    string Title,
    string Grade,
    string Subject,
    string? Term,
    string City,
    ListingIntent Intent,
    BookCondition Condition,
    decimal? PriceKes,
    string Description,
    string CoverImageUrl);

public sealed record ImageStubRequest(string? Filename);

public sealed record ImageStubResponse(string Url);

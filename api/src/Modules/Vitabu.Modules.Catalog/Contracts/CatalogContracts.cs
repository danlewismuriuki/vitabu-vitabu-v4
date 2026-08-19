namespace Vitabu.Modules.Catalog.Contracts;

public sealed record CbcTitleCard(
    Guid Id,
    string Code,
    string Title,
    string Grade,
    string Subject,
    string Term,
    string MaterialType,
    string Language);

public sealed record CbcTitlePage(
    IReadOnlyList<CbcTitleCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record SearchCbcTitlesQuery(
    string? Q,
    string? Grade,
    string? Subject,
    int Page = 1,
    int PageSize = 20);

public sealed record SchoolCard(
    Guid Id,
    string Name,
    string City,
    string? ContactName,
    bool IsVerified);

public sealed record SchoolDetail(
    Guid Id,
    string Name,
    string City,
    string? ContactName,
    string? ContactPhoneE164,
    string? ContactEmail,
    bool IsVerified,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record SchoolPage(
    IReadOnlyList<SchoolCard> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record CreateSchoolRequest(
    string Name,
    string City,
    string? ContactName,
    string? ContactPhoneE164,
    string? ContactEmail,
    string? Notes);

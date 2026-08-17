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

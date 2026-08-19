using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Catalog.Contracts;
using Vitabu.Modules.Catalog.Persistence;

namespace Vitabu.Modules.Catalog.Services;

public interface ICatalogReadService
{
    Task<CbcTitlePage> SearchTitlesAsync(SearchCbcTitlesQuery query, CancellationToken ct = default);
}

public sealed class CatalogReadService(ICatalogDbContext catalogDb) : ICatalogReadService
{
    public async Task<CbcTitlePage> SearchTitlesAsync(SearchCbcTitlesQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var q = catalogDb.CbcTitles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLowerInvariant();
            q = q.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Subject.ToLower().Contains(term) ||
                t.Code.ToLower().Contains(term) ||
                t.Grade.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Grade))
        {
            var grade = query.Grade.Trim();
            q = q.Where(t => t.Grade == grade);
        }

        if (!string.IsNullOrWhiteSpace(query.Subject))
        {
            var subject = query.Subject.Trim();
            q = q.Where(t => t.Subject == subject);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(t => t.Grade)
            .ThenBy(t => t.Subject)
            .ThenBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new CbcTitleCard(
                t.Id,
                t.Code,
                t.Title,
                t.Grade,
                t.Subject,
                t.Term,
                t.MaterialType,
                t.Language))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new CbcTitlePage(items, page, pageSize, total, totalPages);
    }
}

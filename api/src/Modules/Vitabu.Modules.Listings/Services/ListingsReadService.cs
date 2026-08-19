using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Catalog.Persistence;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Persistence;

namespace Vitabu.Modules.Listings.Services;

public interface IListingsReadService
{
    Task<ListingPage> ListAsync(ListListingsQuery query, CancellationToken ct = default);
    Task<ListingDetail> GetAsync(Guid id, CancellationToken ct = default);
    Task<CatalogFacets> GetFacetsAsync(CancellationToken ct = default);
}

public sealed class ListingsReadService(
    IListingsDbContext listingsDb,
    IIdentityDbContext identityDb,
    ICatalogDbContext catalogDb) : IListingsReadService
{
    public async Task<ListingPage> ListAsync(ListListingsQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var q = listingsDb.Listings.AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLowerInvariant();
            q = q.Where(l =>
                l.Title.ToLower().Contains(term) ||
                l.Subject.ToLower().Contains(term) ||
                l.Grade.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Grade))
        {
            var grade = query.Grade.Trim();
            q = q.Where(l => l.Grade == grade);
        }

        if (!string.IsNullOrWhiteSpace(query.Subject))
        {
            var subject = query.Subject.Trim();
            q = q.Where(l => l.Subject == subject);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            q = q.Where(l => l.City == city);
        }

        if (query.Intent is { } intent)
        {
            q = q.Where(l => l.Intent == intent);
        }

        if (query.Condition is { } condition)
        {
            q = q.Where(l => l.Condition == condition);
        }

        if (query.SchoolId is { } schoolId)
        {
            q = q.Where(l => l.SchoolId == schoolId);
        }

        var total = await q.CountAsync(ct);
        var rows = await q
            .OrderByDescending(l => l.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var schoolIds = rows.Where(l => l.SchoolId is not null).Select(l => l.SchoolId!.Value).Distinct().ToList();
        var schools = schoolIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await catalogDb.Schools.AsNoTracking()
                .Where(s => schoolIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var items = rows.Select(l => new ListingCard(
            l.Id,
            l.Title,
            l.Grade,
            l.Subject,
            l.Term,
            l.City,
            l.Intent,
            l.Condition,
            l.Status,
            l.PriceKes,
            l.CoverImageUrl,
            l.InterestCount,
            l.CreatedAtUtc,
            l.SchoolId,
            l.SchoolId is { } sid && schools.TryGetValue(sid, out var name) ? name : null)).ToList();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new ListingPage(items, page, pageSize, total, totalPages);
    }

    public async Task<ListingDetail> GetAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.Status == ListingStatus.Active, ct)
            ?? throw NotFoundException.For("listing", id);

        var seller = await identityDb.Users.AsNoTracking()
            .Where(u => u.Id == listing.SellerUserId)
            .Select(u => new SellerSnippet(u.DisplayName, u.City))
            .FirstOrDefaultAsync(ct)
            ?? new SellerSnippet("Vitabu parent", listing.City);

        SchoolSnippet? school = null;
        if (listing.SchoolId is { } schoolId)
        {
            school = await catalogDb.Schools.AsNoTracking()
                .Where(s => s.Id == schoolId)
                .Select(s => new SchoolSnippet(s.Id, s.Name, s.City))
                .FirstOrDefaultAsync(ct);
        }

        return new ListingDetail(
            listing.Id,
            listing.Title,
            listing.Grade,
            listing.Subject,
            listing.Term,
            listing.City,
            listing.Intent,
            listing.Condition,
            listing.Status,
            listing.PriceKes,
            listing.CoverImageUrl,
            listing.InterestCount,
            listing.CreatedAtUtc,
            listing.Description,
            listing.Slug,
            seller,
            school);
    }

    public async Task<CatalogFacets> GetFacetsAsync(CancellationToken ct = default)
    {
        var active = listingsDb.Listings.AsNoTracking().Where(l => l.Status == ListingStatus.Active);
        var grades = await active.Select(l => l.Grade).Distinct().OrderBy(x => x).ToListAsync(ct);
        var subjects = await active.Select(l => l.Subject).Distinct().OrderBy(x => x).ToListAsync(ct);
        var cities = await active.Select(l => l.City).Distinct().OrderBy(x => x).ToListAsync(ct);

        return new CatalogFacets(
            grades,
            subjects,
            cities,
            ["sale", "free", "exchange", "donate_school"],
            ["like_new", "good", "fair", "writing_inside"]);
    }
}

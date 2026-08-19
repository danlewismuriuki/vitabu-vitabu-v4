using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Admin.Contracts;
using Vitabu.Modules.Deals.Contracts;
using Vitabu.Modules.Deals.Persistence;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Services;

namespace Vitabu.Modules.Admin.Services;

public interface IAdminService
{
    Task EnsureStaffAsync(Guid userId, CancellationToken ct = default);
    Task<AdminListingPage> ListListingsAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task HideListingAsync(Guid staffUserId, Guid listingId, CancellationToken ct = default);
    Task<ListingReportPage> ListReportsAsync(string? status, int page, int pageSize, CancellationToken ct = default);
    Task ResolveReportAsync(Guid staffUserId, Guid reportId, ResolveReportRequest request, CancellationToken ct = default);
}

public sealed class AdminService(
    IIdentityDbContext identityDb,
    IListingsDbContext listingsDb,
    IDealsDbContext dealsDb,
    INotificationService notifications) : IAdminService
{
    public async Task EnsureStaffAsync(Guid userId, CancellationToken ct = default)
    {
        var staff = await identityDb.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.IsStaff, ct);
        if (!staff)
        {
            throw new ForbiddenDomainException("staff_required", "Staff access required.");
        }
    }

    public async Task<AdminListingPage> ListListingsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var q = listingsDb.Listings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().Replace("-", "_");
            foreach (var name in Enum.GetNames<ListingStatus>())
            {
                var snake = ToSnake(name);
                if (snake.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    var st = Enum.Parse<ListingStatus>(name);
                    q = q.Where(l => l.Status == st);
                    break;
                }
            }
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(l => l.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AdminListingCard(
                l.Id,
                l.Title,
                l.City,
                l.Status,
                l.Intent,
                l.SellerUserId,
                l.CreatedAtUtc,
                l.InterestCount))
            .ToListAsync(ct);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new AdminListingPage(items, page, pageSize, total, totalPages);
    }

    public async Task HideListingAsync(Guid staffUserId, Guid listingId, CancellationToken ct = default)
    {
        await EnsureStaffAsync(staffUserId, ct);
        var listing = await listingsDb.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        listing.Status = ListingStatus.Hidden;
        listing.UpdatedAtUtc = DateTime.UtcNow;
        await listingsDb.SaveChangesAsync(ct);

        await notifications.NotifyAsync(
            listing.SellerUserId,
            "listing_hidden",
            "Listing hidden by staff",
            $"“{listing.Title}” was removed from Browse by Vitabu staff.",
            listing.Id,
            "Listing hidden on Vitabu Vitabu",
            $"Your listing “{listing.Title}” was hidden by staff after a report or review.",
            ct);
    }

    public async Task<ListingReportPage> ListReportsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var st = string.IsNullOrWhiteSpace(status) ? "open" : status.Trim().ToLowerInvariant();

        var q = from r in dealsDb.ListingReports.AsNoTracking()
                join l in listingsDb.Listings.AsNoTracking() on r.ListingId equals l.Id
                where r.Status == st
                select new { r, l };

        var total = await q.CountAsync(ct);
        var rows = await q
            .OrderByDescending(x => x.r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(x => new ListingReportItem(
            x.r.Id,
            x.r.ListingId,
            x.l.Title,
            x.r.ReporterUserId,
            x.r.Reason,
            x.r.Details,
            x.r.Status,
            x.r.CreatedAtUtc)).ToList();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new ListingReportPage(items, page, pageSize, total, totalPages);
    }

    public async Task ResolveReportAsync(
        Guid staffUserId,
        Guid reportId,
        ResolveReportRequest request,
        CancellationToken ct = default)
    {
        await EnsureStaffAsync(staffUserId, ct);
        var report = await dealsDb.ListingReports
            .FirstOrDefaultAsync(r => r.Id == reportId, ct)
            ?? throw NotFoundException.For("report", reportId);

        var action = (request.Action ?? "").Trim().ToLowerInvariant();
        if (action is not ("dismiss" or "hide"))
        {
            throw new DomainException("invalid_action", "Action must be dismiss or hide.");
        }

        if (action == "hide")
        {
            await HideListingAsync(staffUserId, report.ListingId, ct);
        }

        report.Status = action == "hide" ? "resolved_hidden" : "dismissed";
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = staffUserId;
        await dealsDb.SaveChangesAsync(ct);
    }

    private static string ToSnake(string pascal)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c) && i > 0) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}

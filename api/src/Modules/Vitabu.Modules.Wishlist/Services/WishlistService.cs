using Microsoft.EntityFrameworkCore;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Listings.Contracts;
using Vitabu.Modules.Listings.Domain;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Wishlist.Contracts;
using Vitabu.Modules.Wishlist.Entities;
using Vitabu.Modules.Wishlist.Persistence;

namespace Vitabu.Modules.Wishlist.Services;

public interface IWishlistService
{
    Task AddAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task<WishlistStatus> GetStatusAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task<WishlistPage> ListMineAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}

public sealed class WishlistService(
    IWishlistDbContext wishlistDb,
    IListingsDbContext listingsDb) : IWishlistService
{
    public async Task AddAsync(Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var listing = await listingsDb.Listings.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == listingId, ct)
            ?? throw NotFoundException.For("listing", listingId);

        if (listing.SellerUserId == userId)
        {
            throw new ValidationException(
                "You cannot wishlist your own listing.",
                new Dictionary<string, string[]>
                {
                    ["listing_id"] = ["Cannot wishlist your own listing."]
                });
        }

        if (listing.Status != ListingStatus.Active)
        {
            throw new ValidationException(
                "Only active listings can be wishlisted.",
                new Dictionary<string, string[]>
                {
                    ["listing_id"] = ["Listing is not active."]
                });
        }

        var exists = await wishlistDb.WishlistEntries.AnyAsync(
            w => w.UserId == userId && w.ListingId == listingId, ct);
        if (exists)
        {
            return;
        }

        wishlistDb.WishlistEntries.Add(new WishlistEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ListingId = listingId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await wishlistDb.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var entry = await wishlistDb.WishlistEntries
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        if (entry is null)
        {
            return;
        }

        wishlistDb.WishlistEntries.Remove(entry);
        await wishlistDb.SaveChangesAsync(ct);
    }

    public async Task<WishlistStatus> GetStatusAsync(
        Guid userId,
        Guid listingId,
        CancellationToken ct = default)
    {
        var on = await wishlistDb.WishlistEntries.AsNoTracking()
            .AnyAsync(w => w.UserId == userId && w.ListingId == listingId, ct);
        return new WishlistStatus(on);
    }

    public async Task<WishlistPage> ListMineAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var q = wishlistDb.WishlistEntries.AsNoTracking()
            .Where(w => w.UserId == userId);

        var total = await q.CountAsync(ct);
        var entries = await q
            .OrderByDescending(w => w.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var listingIds = entries.Select(e => e.ListingId).ToList();
        var listings = await listingsDb.Listings.AsNoTracking()
            .Where(l => listingIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        var items = new List<WishlistItem>(entries.Count);
        foreach (var entry in entries)
        {
            if (!listings.TryGetValue(entry.ListingId, out var listing))
            {
                continue;
            }

            items.Add(new WishlistItem(
                entry.ListingId,
                entry.CreatedAtUtc,
                new ListingCard(
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
                    listing.CreatedAtUtc)));
        }

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new WishlistPage(items, page, pageSize, total, totalPages);
    }
}

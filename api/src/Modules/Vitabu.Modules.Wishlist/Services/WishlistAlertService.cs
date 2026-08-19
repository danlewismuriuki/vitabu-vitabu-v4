using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vitabu.Core.Abstractions;
using Vitabu.Modules.Identity.Persistence;
using Vitabu.Modules.Listings.Persistence;
using Vitabu.Modules.Notifications.Services;
using Vitabu.Modules.Wishlist.Persistence;

namespace Vitabu.Modules.Wishlist.Services;

public sealed class WishlistAlertService(
    IWishlistDbContext wishlistDb,
    IListingsDbContext listingsDb,
    IIdentityDbContext identityDb,
    INotificationService notifications,
    ILogger<WishlistAlertService> logger) : IWishlistAlertService
{
    public async Task NotifySimilarListingCreatedAsync(
        Guid listingId,
        Guid sellerUserId,
        string title,
        string grade,
        string subject,
        string city,
        CancellationToken ct = default)
    {
        try
        {
            var candidateUserIds = await (
                from w in wishlistDb.WishlistEntries.AsNoTracking()
                join l in listingsDb.Listings.AsNoTracking() on w.ListingId equals l.Id
                where w.UserId != sellerUserId
                      && l.Grade == grade
                      && l.Subject == subject
                select w.UserId
            ).Distinct().ToListAsync(ct);

            if (candidateUserIds.Count == 0)
            {
                return;
            }

            var recipients = await identityDb.Users.AsNoTracking()
                .Where(u => candidateUserIds.Contains(u.Id) && u.WishlistAlertsEnabled)
                .Select(u => u.Id)
                .ToListAsync(ct);

            foreach (var userId in recipients)
            {
                await notifications.NotifyAsync(
                    userId,
                    "wishlist_similar_listing",
                    "New book similar to your wishlist",
                    $"A new {grade} {subject} listing is up: “{title}” in {city}.",
                    listingId,
                    "New Vitabu book matched your wishlist",
                    $"A new {grade} {subject} book was listed: “{title}” ({city}). Open Vitabu Vitabu to view it.",
                    ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Wishlist similar-listing alerts failed for {ListingId}", listingId);
        }
    }

    public async Task NotifyWishlistedListingUnavailableAsync(
        Guid listingId,
        string title,
        string reason,
        CancellationToken ct = default)
    {
        try
        {
            var userIds = await wishlistDb.WishlistEntries.AsNoTracking()
                .Where(w => w.ListingId == listingId)
                .Select(w => w.UserId)
                .Distinct()
                .ToListAsync(ct);

            if (userIds.Count == 0)
            {
                return;
            }

            var recipients = await identityDb.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id) && u.WishlistAlertsEnabled)
                .Select(u => u.Id)
                .ToListAsync(ct);

            foreach (var userId in recipients)
            {
                await notifications.NotifyAsync(
                    userId,
                    "wishlist_listing_unavailable",
                    "Saved book no longer available",
                    $"“{title}” on your wishlist is now {reason}.",
                    listingId,
                    "Vitabu wishlist update",
                    $"A book you saved (“{title}”) is now {reason}. Browse similar CBC titles on Vitabu Vitabu.",
                    ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Wishlist unavailable alerts failed for {ListingId}", listingId);
        }
    }
}

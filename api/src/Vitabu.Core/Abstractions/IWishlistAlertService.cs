namespace Vitabu.Core.Abstractions;

/// <summary>
/// Wishlist match / status alerts (in-app + email). Implemented by the Wishlist module.
/// </summary>
public interface IWishlistAlertService
{
    Task NotifySimilarListingCreatedAsync(
        Guid listingId,
        Guid sellerUserId,
        string title,
        string grade,
        string subject,
        string city,
        CancellationToken ct = default);

    Task NotifyWishlistedListingUnavailableAsync(
        Guid listingId,
        string title,
        string reason,
        CancellationToken ct = default);
}

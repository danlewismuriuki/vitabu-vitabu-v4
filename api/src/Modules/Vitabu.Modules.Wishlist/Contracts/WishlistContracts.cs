using Vitabu.Modules.Listings.Contracts;

namespace Vitabu.Modules.Wishlist.Contracts;

public sealed record WishlistStatus(bool OnWishlist);

public sealed record WishlistItem(Guid ListingId, DateTime SavedAtUtc, ListingCard Listing);

public sealed record WishlistPage(
    IReadOnlyList<WishlistItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

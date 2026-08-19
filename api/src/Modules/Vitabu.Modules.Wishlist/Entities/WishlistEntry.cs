namespace Vitabu.Modules.Wishlist.Entities;

public sealed class WishlistEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

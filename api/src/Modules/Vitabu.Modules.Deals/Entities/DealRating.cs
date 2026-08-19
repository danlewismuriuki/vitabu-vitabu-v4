namespace Vitabu.Modules.Deals.Entities;

public sealed class DealRating
{
    public Guid Id { get; set; }
    public Guid InterestId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

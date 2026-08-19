namespace Vitabu.Modules.Catalog.Entities;

public sealed class School
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhoneE164 { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsVerified { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

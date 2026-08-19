namespace Vitabu.Modules.Identity.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PhoneE164 { get; set; }
    public DateTime? PhoneVerifiedAtUtc { get; set; }
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime AcceptedTermsAtUtc { get; set; }
    public bool ConfirmedParentGuardian { get; set; }
    public bool IsStaff { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

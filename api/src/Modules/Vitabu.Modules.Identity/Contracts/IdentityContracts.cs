namespace Vitabu.Modules.Identity.Contracts;

public sealed record RegisterRequest(
    string DisplayName,
    string Email,
    string Password,
    string City,
    bool AcceptTerms,
    bool ConfirmParentGuardian);

public sealed record LoginRequest(string Email, string Password);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string Password);

public sealed record RequestPhoneOtpRequest(string PhoneE164);

public sealed record VerifyPhoneOtpRequest(string Code);

public sealed record UserProfile(
    Guid Id,
    string DisplayName,
    string Email,
    string City,
    string? PhoneE164,
    bool PhoneVerified,
    DateTime? PhoneVerifiedAtUtc,
    bool EmailVerified,
    DateTime CreatedAtUtc);

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    UserProfile User);

public sealed record MessageResponse(string Message);

public sealed record RequestPhoneOtpResponse(
    string Message,
    int ExpiresInSeconds,
    string? DevCode);

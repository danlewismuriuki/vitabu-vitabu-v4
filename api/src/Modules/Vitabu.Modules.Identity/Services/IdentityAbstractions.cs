using Vitabu.Modules.Identity.Entities;

namespace Vitabu.Modules.Identity.Services;

public interface IJwtTokenService
{
    (string Token, int ExpiresInSeconds) CreateToken(User user);
}

public interface ISmsSender
{
    Task SendAsync(string phoneE164, string message, CancellationToken cancellationToken = default);
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}

public interface IIdentityService
{
    Task<Contracts.AuthResponse> RegisterAsync(Contracts.RegisterRequest request, CancellationToken ct = default);
    Task<Contracts.AuthResponse> LoginAsync(Contracts.LoginRequest request, CancellationToken ct = default);
    Task<Contracts.UserProfile> GetMeAsync(Guid userId, CancellationToken ct = default);
    Task<Contracts.UserProfile> UpdateNotificationPrefsAsync(
        Guid userId,
        Contracts.UpdateNotificationPrefsRequest request,
        CancellationToken ct = default);
    Task<Contracts.MessageResponse> ForgotPasswordAsync(Contracts.ForgotPasswordRequest request, CancellationToken ct = default);
    Task<Contracts.MessageResponse> ResetPasswordAsync(Contracts.ResetPasswordRequest request, CancellationToken ct = default);
    Task<Contracts.RequestPhoneOtpResponse> RequestPhoneOtpAsync(Guid userId, Contracts.RequestPhoneOtpRequest request, CancellationToken ct = default);
    Task<Contracts.UserProfile> VerifyPhoneOtpAsync(Guid userId, Contracts.VerifyPhoneOtpRequest request, CancellationToken ct = default);
}

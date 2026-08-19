using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Vitabu.Core.Exceptions;
using Vitabu.Modules.Identity.Contracts;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Identity.Persistence;

namespace Vitabu.Modules.Identity.Services;

public sealed class IdentityService(
    IIdentityDbContext db,
    IJwtTokenService jwt,
    ISmsSender sms,
    IEmailSender email,
    IHostEnvironment environment,
    IPasswordHasher<User> passwordHasher) : IIdentityService
{
    private const int OtpTtlMinutes = 10;
    private const int ResetTtlHours = 2;
    private const int MaxOtpAttempts = 5;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalized = NormalizeEmail(request.Email);
        if (await db.Users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
        {
            throw new ConflictException("email_taken", "An account with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalized,
            DisplayName = request.DisplayName.Trim(),
            City = request.City.Trim(),
            AcceptedTermsAtUtc = now,
            ConfirmedParentGuardian = request.ConfirmParentGuardian,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalized = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is null)
        {
            throw new UnauthorizedDomainException("invalid_credentials", "Email or password is incorrect.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedDomainException("invalid_credentials", "Email or password is incorrect.");
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return CreateAuthResponse(user);
    }

    public async Task<UserProfile> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);
        return MapProfile(user);
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var normalized = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is not null)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var entity = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = Hash(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddHours(ResetTtlHours),
                CreatedAtUtc = DateTime.UtcNow
            };
            db.PasswordResetTokens.Add(entity);
            await db.SaveChangesAsync(ct);

            var resetUrl = $"http://localhost:3000/reset-password?token={Uri.EscapeDataString(rawToken)}";
            await email.SendAsync(
                user.Email,
                "Reset your Vitabu Vitabu password",
                $"Hi {user.DisplayName},\n\nReset your password: {resetUrl}\n\nThis link expires in {ResetTtlHours} hours.\n",
                ct);
        }

        return new MessageResponse("If that email exists, we sent reset instructions.");
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var hash = Hash(request.Token);
        var token = await db.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => t.TokenHash == hash && t.ConsumedAtUtc == null)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (token is null || token.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new DomainException("reset_token_invalid", "This reset link is invalid or has expired.");
        }

        token.ConsumedAtUtc = DateTime.UtcNow;
        token.User.PasswordHash = passwordHasher.HashPassword(token.User, request.Password);
        token.User.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new MessageResponse("Password updated. You can log in now.");
    }

    public async Task<RequestPhoneOtpResponse> RequestPhoneOtpAsync(
        Guid userId,
        RequestPhoneOtpRequest request,
        CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);
        var phone = request.PhoneE164.Trim();

        var taken = await db.Users.AnyAsync(
            u => u.PhoneE164 == phone && u.PhoneVerifiedAtUtc != null && u.Id != userId,
            ct);
        if (taken)
        {
            throw new ConflictException("phone_taken", "This phone number is already verified on another account.");
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challenge = new PhoneOtpChallenge
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PhoneE164 = phone,
            CodeHash = Hash(code),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpTtlMinutes),
            CreatedAtUtc = DateTime.UtcNow
        };

        // Invalidate prior open challenges for this user
        var open = await db.PhoneOtpChallenges
            .Where(c => c.UserId == userId && c.ConsumedAtUtc == null)
            .ToListAsync(ct);
        foreach (var c in open)
        {
            c.ConsumedAtUtc = DateTime.UtcNow;
        }

        user.PhoneE164 = phone;
        user.PhoneVerifiedAtUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        db.PhoneOtpChallenges.Add(challenge);
        await db.SaveChangesAsync(ct);

        await sms.SendAsync(phone, $"Your Vitabu Vitabu code is {code}. Valid for {OtpTtlMinutes} minutes.", ct);

        return new RequestPhoneOtpResponse(
            "OTP sent.",
            OtpTtlMinutes * 60,
            environment.IsDevelopment() ? code : null);
    }

    public async Task<UserProfile> VerifyPhoneOtpAsync(
        Guid userId,
        VerifyPhoneOtpRequest request,
        CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);
        var challenge = await db.PhoneOtpChallenges
            .Where(c => c.UserId == userId && c.ConsumedAtUtc == null)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (challenge is null)
        {
            throw new DomainException("otp_invalid", "No active verification code. Request a new one.");
        }

        if (challenge.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new DomainException("otp_expired", "This code has expired. Request a new one.");
        }

        if (challenge.AttemptCount >= MaxOtpAttempts)
        {
            challenge.ConsumedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            throw new DomainException("otp_invalid", "Too many attempts. Request a new code.");
        }

        challenge.AttemptCount++;
        if (!FixedTimeEquals(challenge.CodeHash, Hash(request.Code)))
        {
            await db.SaveChangesAsync(ct);
            throw new DomainException("otp_invalid", "Incorrect code.");
        }

        var phone = challenge.PhoneE164;
        var taken = await db.Users.AnyAsync(
            u => u.PhoneE164 == phone && u.PhoneVerifiedAtUtc != null && u.Id != userId,
            ct);
        if (taken)
        {
            throw new ConflictException("phone_taken", "This phone number is already verified on another account.");
        }

        challenge.ConsumedAtUtc = DateTime.UtcNow;
        user.PhoneE164 = phone;
        user.PhoneVerifiedAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return MapProfile(user);
    }

    private async Task<User> GetUserOrThrowAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user ?? throw NotFoundException.For("user", userId);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var (token, expiresIn) = jwt.CreateToken(user);
        return new AuthResponse(token, "Bearer", expiresIn, MapProfile(user));
    }

    private static UserProfile MapProfile(User user) => new(
        user.Id,
        user.DisplayName,
        user.Email,
        user.City,
        user.PhoneE164,
        user.PhoneVerifiedAtUtc != null,
        user.PhoneVerifiedAtUtc,
        user.EmailVerifiedAtUtc != null,
        user.IsStaff,
        user.CreatedAtUtc);

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}

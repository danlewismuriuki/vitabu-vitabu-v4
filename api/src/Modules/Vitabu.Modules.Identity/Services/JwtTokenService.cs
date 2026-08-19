using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vitabu.Modules.Identity.Entities;

namespace Vitabu.Modules.Identity.Services;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string Token, int ExpiresInSeconds) CreateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwtSection["Issuer"] ?? "vitabu-api";
        var audience = jwtSection["Audience"] ?? "vitabu-web";
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60 * 24 * 7;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("display_name", user.DisplayName),
            new("phone_verified", (user.PhoneVerifiedAtUtc != null).ToString().ToLowerInvariant()),
            new("is_staff", user.IsStaff.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var expiresIn = (int)TimeSpan.FromMinutes(expiresMinutes).TotalSeconds;
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresIn);
    }
}

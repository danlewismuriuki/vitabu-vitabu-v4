using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Identity.Entities;

namespace Vitabu.Modules.Identity.Persistence;

public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    DbSet<PhoneOtpChallenge> PhoneOtpChallenges { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

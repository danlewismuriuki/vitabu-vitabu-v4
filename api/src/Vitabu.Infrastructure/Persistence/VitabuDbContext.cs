using Microsoft.EntityFrameworkCore;
using Vitabu.Modules.Identity.Entities;
using Vitabu.Modules.Identity.Persistence;

namespace Vitabu.Infrastructure.Persistence;

public sealed class VitabuDbContext(DbContextOptions<VitabuDbContext> options)
    : DbContext(options), IIdentityDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PhoneOtpChallenge> PhoneOtpChallenges => Set<PhoneOtpChallenge>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

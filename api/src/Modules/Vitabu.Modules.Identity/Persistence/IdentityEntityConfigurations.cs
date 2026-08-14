using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitabu.Modules.Identity.Entities;

namespace Vitabu.Modules.Identity.Persistence;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(80).IsRequired();
        builder.Property(x => x.City).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PhoneE164).HasMaxLength(20);
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasIndex(x => x.PhoneE164)
            .IsUnique()
            .HasFilter("\"PhoneE164\" IS NOT NULL");
    }
}

public sealed class PhoneOtpChallengeConfiguration : IEntityTypeConfiguration<PhoneOtpChallenge>
{
    public void Configure(EntityTypeBuilder<PhoneOtpChallenge> builder)
    {
        builder.ToTable("phone_otp_challenges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PhoneE164).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
    }
}

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.TokenHash);
    }
}

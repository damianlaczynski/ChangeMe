using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserRecoveryCodeConfiguration : IEntityTypeConfiguration<UserRecoveryCode>
{
  public void Configure(EntityTypeBuilder<UserRecoveryCode> builder)
  {
    builder.ToTable("user_recovery_codes");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.CodeHash)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.RECOVERY_CODE_HASH_MAX_LENGTH);

    builder.Property(x => x.UsedAtUtc);

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => new { x.UserId, x.CodeHash })
      .IsUnique();

    builder.HasOne(x => x.User)
      .WithMany(x => x.RecoveryCodes)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

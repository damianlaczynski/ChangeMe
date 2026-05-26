using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class TwoFactorEnrollmentPendingConfiguration : IEntityTypeConfiguration<TwoFactorEnrollmentPending>
{
  public void Configure(EntityTypeBuilder<TwoFactorEnrollmentPending> builder)
  {
    builder.ToTable("two_factor_enrollment_pending");

    builder.HasKey(x => x.UserId);

    builder.Property(x => x.SecretCiphertext)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH);

    builder.Property(x => x.ExpiresAtUtc)
      .IsRequired();

    builder.HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

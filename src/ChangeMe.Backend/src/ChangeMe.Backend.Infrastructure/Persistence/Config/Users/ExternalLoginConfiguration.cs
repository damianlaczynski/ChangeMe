using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
  public void Configure(EntityTypeBuilder<ExternalLogin> builder)
  {
    builder.ToTable("external_logins");

    builder.HasKey(x => new { x.UserId, x.ProviderKey });

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.ProviderKey)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);

    builder.Property(x => x.ProviderSubject)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.PROVIDER_SUBJECT_MAX_LENGTH);

    builder.Property(x => x.LinkedAtUtc)
      .IsRequired();

    builder.Property(x => x.LastStepUpAtUtc);

    builder.Property(x => x.LastProviderEmail)
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.HasIndex(x => new { x.ProviderKey, x.ProviderSubject })
      .IsUnique();

    builder.HasOne(x => x.User)
      .WithMany(x => x.ExternalLogins)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

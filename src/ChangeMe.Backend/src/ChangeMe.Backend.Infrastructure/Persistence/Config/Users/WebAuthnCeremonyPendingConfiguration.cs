using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class WebAuthnCeremonyPendingConfiguration : IEntityTypeConfiguration<WebAuthnCeremonyPending>
{
  public void Configure(EntityTypeBuilder<WebAuthnCeremonyPending> builder)
  {
    builder.ToTable("webauthn_ceremony_pending");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId);
    builder.Property(x => x.NormalizedEmail).HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);
    builder.Property(x => x.CeremonyType).IsRequired();
    builder.Property(x => x.OptionsJson)
      .IsRequired()
      .HasMaxLength(WebAuthnCeremonyPendingConstraints.OPTIONS_JSON_MAX_LENGTH);
    builder.Property(x => x.FailedAttemptCount).IsRequired();
    builder.Property(x => x.ExpiresAtUtc).IsRequired();

    builder.HasIndex(x => x.ExpiresAtUtc);
  }
}

using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class ExternalAuthPendingConfiguration : IEntityTypeConfiguration<ExternalAuthPending>
{
  public void Configure(EntityTypeBuilder<ExternalAuthPending> builder)
  {
    builder.ToTable("external_auth_pending");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.ProviderKey)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);

    builder.Property(x => x.State)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(x => x.Nonce)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(x => x.CodeChallenge)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(x => x.CodeVerifier)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(x => x.Mode)
      .IsRequired()
      .HasConversion<int>();

    builder.Property(x => x.ProviderSubject)
      .HasMaxLength(TwoFactorConstraints.PROVIDER_SUBJECT_MAX_LENGTH);

    builder.Property(x => x.ProviderEmail)
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.InvitedProfileEmail)
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.ProviderFirstName)
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.ProviderLastName)
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.ExpiresAtUtc)
      .IsRequired();

    builder.HasIndex(x => x.State)
      .IsUnique();

    builder.HasIndex(x => x.ExpiresAtUtc);
  }
}

using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
  public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
  {
    builder.ToTable("passkey_credentials");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.Name)
      .IsRequired()
      .HasMaxLength(PasskeyConstraints.NAME_MAX_LENGTH);
    builder.Property(x => x.CredentialId)
      .IsRequired()
      .HasMaxLength(PasskeyConstraints.CREDENTIAL_ID_MAX_LENGTH);
    builder.Property(x => x.PublicKey)
      .IsRequired()
      .HasMaxLength(PasskeyConstraints.PUBLIC_KEY_MAX_LENGTH);
    builder.Property(x => x.SignCount).IsRequired();
    builder.Property(x => x.Aaguid).IsRequired();
    builder.Property(x => x.AuthenticatorType)
      .IsRequired()
      .HasMaxLength(PasskeyConstraints.AUTHENTICATOR_TYPE_MAX_LENGTH);
    builder.Property(x => x.BackupEligible).IsRequired();
    builder.Property(x => x.BackupState).IsRequired();
    builder.Property(x => x.CreatedAtUtc).IsRequired();
    builder.Property(x => x.LastUsedAtUtc);

    builder.HasIndex(x => x.CredentialId).IsUnique();

    builder.HasOne(x => x.User)
      .WithMany(x => x.Passkeys)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

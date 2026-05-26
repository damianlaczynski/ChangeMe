using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserConfiguration : BaseEntityTypeConfiguration<User>
{
  protected override string TableName => "users";

  public override void Configure(EntityTypeBuilder<User> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.FirstName)
      .IsRequired()
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.LastName)
      .IsRequired()
      .HasMaxLength(UserConstraints.NAME_MAX_LENGTH);

    builder.Property(x => x.Email)
      .IsRequired()
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.NormalizedEmail)
      .IsRequired()
      .HasMaxLength(UserConstraints.EMAIL_MAX_LENGTH);

    builder.Property(x => x.PasswordHash)
      .IsRequired()
      .HasMaxLength(512);

    builder.Property(x => x.Deactivated)
      .IsRequired();

    builder.Property(x => x.DeactivatedAt);

    builder.Property(x => x.HasPasswordSet)
      .IsRequired();

    builder.Property(x => x.EmailVerified)
      .IsRequired();

    builder.Property(x => x.EmailVerifiedAt);
    builder.Property(x => x.PasswordLastChangedAt);

    builder.Property(x => x.TwoFactorEnabled)
      .IsRequired();

    builder.Property(x => x.TwoFactorEnabledAt);

    builder.Property(x => x.TwoFactorSecretCiphertext)
      .IsRequired()
      .HasMaxLength(TwoFactorConstraints.ENCRYPTED_SECRET_MAX_LENGTH);

    builder.Property(x => x.PasskeyStepUpCompletedAt);

    builder.HasIndex(x => x.NormalizedEmail)
      .IsUnique();

    builder.HasMany(x => x.Roles)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.Roles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.ExternalLogins)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.ExternalLogins)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.RecoveryCodes)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.RecoveryCodes)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.AccountInvitations)
      .WithOne()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.AccountInvitations)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.Passkeys)
      .WithOne(x => x.User)
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(x => x.Passkeys)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

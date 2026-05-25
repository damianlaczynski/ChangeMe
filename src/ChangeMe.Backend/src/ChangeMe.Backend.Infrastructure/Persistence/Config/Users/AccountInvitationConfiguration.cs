using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class AccountInvitationConfiguration : IEntityTypeConfiguration<AccountInvitation>
{
  public void Configure(EntityTypeBuilder<AccountInvitation> builder)
  {
    builder.ToTable("account_invitations");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.SentAtUtc)
      .IsRequired();

    builder.Property(x => x.LinkExpiresAtUtc)
      .IsRequired();

    builder.Property(x => x.AcceptedAtUtc);
    builder.Property(x => x.RevokedAtUtc);

    builder.HasIndex(x => new { x.UserId, x.SentAtUtc });
  }
}

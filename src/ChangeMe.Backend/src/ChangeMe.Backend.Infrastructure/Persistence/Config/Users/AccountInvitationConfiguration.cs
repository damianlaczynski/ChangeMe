using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class AccountInvitationConfiguration : BaseEntityTypeConfiguration<AccountInvitation>
{
  protected override string TableName => "account_invitations";

  public override void Configure(EntityTypeBuilder<AccountInvitation> builder)
  {
    base.Configure(builder);

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

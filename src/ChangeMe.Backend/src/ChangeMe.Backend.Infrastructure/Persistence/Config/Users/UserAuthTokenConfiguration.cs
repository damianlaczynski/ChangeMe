using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserAuthTokenConfiguration : BaseEntityTypeConfiguration<UserAuthToken>
{
  protected override string TableName => "user_auth_tokens";

  public override void Configure(EntityTypeBuilder<UserAuthToken> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.Type)
      .IsRequired()
      .HasConversion<int>();

    builder.Property(x => x.TokenHash)
      .IsRequired()
      .HasMaxLength(UserAuthTokenConstraints.TOKEN_HASH_MAX_LENGTH);

    builder.Property(x => x.ExpiresAtUtc)
      .IsRequired();

    builder.Property(x => x.UsedAtUtc);

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => new { x.UserId, x.Type, x.UsedAtUtc });
    builder.HasIndex(x => x.TokenHash)
      .IsUnique();
  }
}

using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserAuthTokenConfiguration : IEntityTypeConfiguration<UserAuthToken>
{
  public void Configure(EntityTypeBuilder<UserAuthToken> builder)
  {
    builder.ToTable("user_auth_tokens");

    builder.HasKey(x => x.Id);

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

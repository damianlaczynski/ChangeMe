using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class SignInChallengeConfiguration : IEntityTypeConfiguration<SignInChallenge>
{
  public void Configure(EntityTypeBuilder<SignInChallenge> builder)
  {
    builder.ToTable("sign_in_challenges");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.FailedAttemptCount)
      .IsRequired();

    builder.Property(x => x.ExpiresAtUtc)
      .IsRequired();

    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.ExpiresAtUtc);

    builder.HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

using ChangeMe.Backend.Domain.Aggregates.Time;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Time;

public class UserRunningTimerConfiguration : BaseEntityTypeConfiguration<UserRunningTimer>
{
  protected override string TableName => "user_running_timers";

  public override void Configure(EntityTypeBuilder<UserRunningTimer> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.StartedAtUtc).IsRequired();

    builder.HasIndex(x => x.UserId).IsUnique();
  }
}

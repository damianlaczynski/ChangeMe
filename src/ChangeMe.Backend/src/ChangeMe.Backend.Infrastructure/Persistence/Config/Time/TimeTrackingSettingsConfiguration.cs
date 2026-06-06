using ChangeMe.Backend.Domain.Aggregates.Time;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Time;

public class TimeTrackingSettingsConfiguration : BaseEntityTypeConfiguration<TimeTrackingSettings>
{
  protected override string TableName => "time_tracking_settings";

  public override void Configure(EntityTypeBuilder<TimeTrackingSettings> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.BackdatingLimitDays).IsRequired();
  }
}

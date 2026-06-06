using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class BillingSettingsConfiguration : BaseEntityTypeConfiguration<BillingSettings>
{
  protected override string TableName => "billing_settings";

  public override void Configure(EntityTypeBuilder<BillingSettings> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.DefaultAnnualLeaveDays).HasPrecision(4, 1);
    builder.Property(x => x.AllowHalfDayLeave).IsRequired();
    builder.Property(x => x.DefaultWorkdayStart).IsRequired();
    builder.Property(x => x.DefaultWorkdayEnd).IsRequired();
    builder.Property(x => x.HalfDaySplitTime).IsRequired();
    builder.Property(x => x.DefaultWorkdaysCsv).IsRequired().HasMaxLength(50);
    builder.Property(x => x.DefaultAvailabilityStatus).HasConversion<string>().IsRequired();
  }
}

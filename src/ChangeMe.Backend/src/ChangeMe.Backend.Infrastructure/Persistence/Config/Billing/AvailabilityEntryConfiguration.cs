using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class AvailabilityEntryConfiguration : BaseEntityTypeConfiguration<AvailabilityEntry>
{
  protected override string TableName => "availability_entries";

  public override void Configure(EntityTypeBuilder<AvailabilityEntry> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.StartDate).IsRequired();
    builder.Property(x => x.EndDate).IsRequired();
    builder.Property(x => x.AllDay).IsRequired();
    builder.Property(x => x.Status).HasConversion<string>().IsRequired();
    builder.Property(x => x.Notes).IsRequired().HasMaxLength(BillingConstraints.AvailabilityNotesMaxLength);
    builder.Property(x => x.Source).HasConversion<string>().IsRequired();
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.LeaveRequestId);
  }
}

public class WeeklyRecurringPatternConfiguration : BaseEntityTypeConfiguration<WeeklyRecurringPattern>
{
  protected override string TableName => "weekly_recurring_patterns";

  public override void Configure(EntityTypeBuilder<WeeklyRecurringPattern> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.HasIndex(x => x.UserId).IsUnique();
    builder.HasMany(x => x.Days)
      .WithOne()
      .HasForeignKey(d => d.PatternId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

public class WeeklyRecurringPatternDayConfiguration : BaseEntityTypeConfiguration<WeeklyRecurringPatternDay>
{
  protected override string TableName => "weekly_recurring_pattern_days";

  public override void Configure(EntityTypeBuilder<WeeklyRecurringPatternDay> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.PatternId).IsRequired();
    builder.Property(x => x.DayOfWeek).IsRequired();
    builder.Property(x => x.Enabled).IsRequired();
    builder.Property(x => x.Status).HasConversion<string>();
    builder.HasIndex(x => new { x.PatternId, x.DayOfWeek }).IsUnique();
  }
}

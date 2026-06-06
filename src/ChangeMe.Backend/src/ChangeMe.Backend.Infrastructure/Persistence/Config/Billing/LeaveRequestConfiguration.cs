using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class LeaveRequestConfiguration : BaseEntityTypeConfiguration<LeaveRequest>
{
  protected override string TableName => "leave_requests";

  public override void Configure(EntityTypeBuilder<LeaveRequest> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.LeaveTypeId).IsRequired();
    builder.Property(x => x.StartDate).IsRequired();
    builder.Property(x => x.EndDate).IsRequired();
    builder.Property(x => x.DayPortion).HasConversion<string>();
    builder.Property(x => x.Status).HasConversion<string>().IsRequired();
    builder.Property(x => x.Reason).IsRequired().HasMaxLength(BillingConstraints.LeaveReasonMaxLength);
    builder.Property(x => x.RejectReason).IsRequired().HasMaxLength(BillingConstraints.LeaveRejectReasonMaxLength);
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.LeaveTypeId);
  }
}

public class SettlementPeriodConfiguration : BaseEntityTypeConfiguration<SettlementPeriod>
{
  protected override string TableName => "settlement_periods";

  public override void Configure(EntityTypeBuilder<SettlementPeriod> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.Year).IsRequired();
    builder.Property(x => x.Month).IsRequired();
    builder.Property(x => x.Status).HasConversion<string>().IsRequired();
    builder.HasIndex(x => new { x.Year, x.Month }).IsUnique();
  }
}

public class UserSettlementConfiguration : BaseEntityTypeConfiguration<UserSettlement>
{
  protected override string TableName => "user_settlements";

  public override void Configure(EntityTypeBuilder<UserSettlement> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.SettlementPeriodId).IsRequired();
    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.LeaveDays).HasPrecision(5, 1);
    builder.HasIndex(x => new { x.SettlementPeriodId, x.UserId }).IsUnique();
  }
}

public class SettlementOperationLogEntryConfiguration : BaseEntityTypeConfiguration<SettlementOperationLogEntry>
{
  protected override string TableName => "settlement_operation_log";

  public override void Configure(EntityTypeBuilder<SettlementOperationLogEntry> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.SettlementPeriodId).IsRequired();
    builder.Property(x => x.PeriodYear).IsRequired();
    builder.Property(x => x.PeriodMonth).IsRequired();
    builder.Property(x => x.Operation).HasConversion<string>().IsRequired();
    builder.Property(x => x.ActorUserId).IsRequired();
    builder.Property(x => x.Timestamp).IsRequired();
    builder.HasIndex(x => x.Timestamp);
    builder.HasIndex(x => x.SettlementPeriodId);
  }
}

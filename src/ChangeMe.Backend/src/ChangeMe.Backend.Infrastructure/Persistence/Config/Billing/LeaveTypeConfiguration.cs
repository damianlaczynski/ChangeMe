using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class LeaveTypeConfiguration : BaseEntityTypeConfiguration<LeaveType>
{
  protected override string TableName => "leave_types";

  public override void Configure(EntityTypeBuilder<LeaveType> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.Name).IsRequired().HasMaxLength(BillingConstraints.LeaveTypeNameMaxLength);
    builder.Property(x => x.NormalizedName).IsRequired().HasMaxLength(BillingConstraints.LeaveTypeNameMaxLength);
    builder.Property(x => x.Code).IsRequired().HasMaxLength(BillingConstraints.LeaveTypeCodeMaxLength);
    builder.Property(x => x.NormalizedCode).IsRequired().HasMaxLength(BillingConstraints.LeaveTypeCodeMaxLength);
    builder.HasIndex(x => x.NormalizedName).IsUnique();
    builder.HasIndex(x => x.NormalizedCode).IsUnique();
  }
}

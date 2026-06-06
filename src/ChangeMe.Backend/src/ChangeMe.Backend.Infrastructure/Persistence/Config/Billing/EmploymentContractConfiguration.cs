using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class EmploymentContractConfiguration : BaseEntityTypeConfiguration<EmploymentContract>
{
  protected override string TableName => "employment_contracts";

  public override void Configure(EntityTypeBuilder<EmploymentContract> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.PositionId).IsRequired();
    builder.Property(x => x.ContractType).HasConversion<string>().IsRequired();
    builder.Property(x => x.StartDate).IsRequired();
    builder.Property(x => x.Fte).HasPrecision(3, 2).IsRequired();
    builder.Property(x => x.MonthlyHoursNormMinutes).IsRequired();
    builder.Property(x => x.HourlyRate).HasPrecision(12, 2);
    builder.Property(x => x.MonthlySalary).HasPrecision(12, 2);
    builder.Property(x => x.Notes).IsRequired().HasMaxLength(BillingConstraints.ContractNotesMaxLength);
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.PositionId);
  }
}

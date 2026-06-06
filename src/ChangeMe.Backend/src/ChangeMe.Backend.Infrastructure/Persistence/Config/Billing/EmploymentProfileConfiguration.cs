using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class EmploymentProfileConfiguration : BaseEntityTypeConfiguration<EmploymentProfile>
{
  protected override string TableName => "employment_profiles";

  public override void Configure(EntityTypeBuilder<EmploymentProfile> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId).IsRequired();
    builder.Property(x => x.EmployeeId).IsRequired().HasMaxLength(BillingConstraints.EmployeeIdMaxLength);
    builder.Property(x => x.NormalizedEmployeeId).IsRequired().HasMaxLength(BillingConstraints.EmployeeIdMaxLength);
    builder.Property(x => x.NationalId).IsRequired().HasMaxLength(BillingConstraints.NationalIdMaxLength);
    builder.Property(x => x.TaxId).IsRequired().HasMaxLength(BillingConstraints.TaxIdMaxLength);
    builder.Property(x => x.BankAccount).IsRequired().HasMaxLength(BillingConstraints.BankAccountMaxLength);
    builder.Property(x => x.Notes).IsRequired().HasMaxLength(BillingConstraints.EmploymentNotesMaxLength);
    builder.HasIndex(x => x.UserId).IsUnique();
  }
}

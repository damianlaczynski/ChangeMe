using ChangeMe.Backend.Domain.Aggregates.Billing;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Billing;

public class PositionConfiguration : BaseEntityTypeConfiguration<Position>
{
  protected override string TableName => "positions";

  public override void Configure(EntityTypeBuilder<Position> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.Name).IsRequired().HasMaxLength(BillingConstraints.PositionNameMaxLength);
    builder.Property(x => x.NormalizedName).IsRequired().HasMaxLength(BillingConstraints.PositionNameMaxLength);
    builder.Property(x => x.Department).IsRequired().HasMaxLength(BillingConstraints.PositionDepartmentMaxLength);
    builder.Property(x => x.Description).IsRequired().HasMaxLength(BillingConstraints.PositionDescriptionMaxLength);
    builder.Property(x => x.IsActive).IsRequired();
    builder.HasIndex(x => x.NormalizedName).IsUnique();
  }
}

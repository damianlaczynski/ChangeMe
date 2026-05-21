using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Infrastructure.Common;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Roles;

public class RolePermissionConfiguration : BaseEntityTypeConfiguration<RolePermission>
{
  protected override string TableName => "role_permissions";

  public override void Configure(EntityTypeBuilder<RolePermission> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.RoleId)
      .IsRequired();

    builder.Property(x => x.PermissionCode)
      .HasMaxLength(64)
      .IsRequired();

    builder.HasIndex(x => new { x.RoleId, x.PermissionCode })
      .IsUnique();
  }
}

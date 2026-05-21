using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Common;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Users;

public class UserRoleConfiguration : BaseEntityTypeConfiguration<UserRole>
{
  protected override string TableName => "user_roles";

  public override void Configure(EntityTypeBuilder<UserRole> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.UserId)
      .IsRequired();

    builder.Property(x => x.RoleId)
      .IsRequired();

    builder.HasIndex(x => new { x.UserId, x.RoleId })
      .IsUnique();

    builder.HasOne(x => x.Role)
      .WithMany()
      .HasForeignKey(x => x.RoleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

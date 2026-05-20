using ChangeMe.Backend.Domain.Aggregates.Roles;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Roles;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
  public void Configure(EntityTypeBuilder<UserRole> builder)
  {
    builder.ToTable("user_roles");

    builder.HasKey(x => new { x.UserId, x.RoleId });

    builder.HasOne(x => x.Role)
      .WithMany()
      .HasForeignKey(x => x.RoleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

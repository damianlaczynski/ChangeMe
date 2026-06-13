using ChangeMe.Backend.Domain.Aggregates.Projects.Entities;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Projects;

public class ProjectMemberConfiguration : BaseEntityTypeConfiguration<ProjectMember>
{
  protected override string TableName => "project_members";

  public override void Configure(EntityTypeBuilder<ProjectMember> builder)
  {
    base.Configure(builder);

    builder.Property(m => m.ProjectId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(m => m.UserId)
      .IsRequired()
      .HasColumnType("uuid");

    builder.Property(m => m.Role)
      .IsRequired()
      .HasConversion<string>();

    builder.HasIndex(m => new { m.ProjectId, m.UserId })
      .IsUnique();
  }
}

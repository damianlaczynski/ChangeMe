using ChangeMe.Backend.Domain.Aggregates.Project;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Projects;

public class ProjectConfiguration : BaseEntityTypeConfiguration<Project>
{
  protected override string TableName => "projects";

  public override void Configure(EntityTypeBuilder<Project> builder)
  {
    base.Configure(builder);

    builder.Property(p => p.Name)
      .IsRequired()
      .HasMaxLength(ProjectConstraints.NAME_MAX_LENGTH);

    builder.Property(p => p.NormalizedName)
      .IsRequired()
      .HasMaxLength(ProjectConstraints.NAME_MAX_LENGTH);

    builder.Property(p => p.Description)
      .IsRequired()
      .HasMaxLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH);

    builder.Property(p => p.IsSystem)
      .IsRequired();

    builder.HasIndex(p => p.NormalizedName)
      .IsUnique();

    builder.HasMany(p => p.Members)
      .WithOne()
      .HasForeignKey(m => m.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(p => p.MembershipHistory)
      .WithOne()
      .HasForeignKey(h => h.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(p => p.OperationHistory)
      .WithOne()
      .HasForeignKey(h => h.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

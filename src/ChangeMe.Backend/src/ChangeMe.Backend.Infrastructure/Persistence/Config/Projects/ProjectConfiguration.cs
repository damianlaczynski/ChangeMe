using ChangeMe.Backend.Domain.Aggregates.Projects;

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

    builder.Property(p => p.Key)
      .IsRequired()
      .HasMaxLength(ProjectConstraints.KEY_MAX_LENGTH);

    builder.HasIndex(p => p.Key)
      .IsUnique();

    builder.Property(p => p.Description)
      .HasMaxLength(ProjectConstraints.DESCRIPTION_MAX_LENGTH);

    builder.Property(p => p.Status)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(p => p.Visibility)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(p => p.Color)
      .IsRequired()
      .HasMaxLength(7);

    builder.HasMany(p => p.Members)
      .WithOne()
      .HasForeignKey(m => m.ProjectId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

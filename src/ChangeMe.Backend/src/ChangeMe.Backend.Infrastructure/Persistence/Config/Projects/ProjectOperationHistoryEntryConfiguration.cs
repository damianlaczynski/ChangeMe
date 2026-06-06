using ChangeMe.Backend.Domain.Aggregates.Project.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Projects;

public class ProjectOperationHistoryEntryConfiguration : BaseEntityTypeConfiguration<ProjectOperationHistoryEntry>
{
  protected override string TableName => "project_operation_history";

  public override void Configure(EntityTypeBuilder<ProjectOperationHistoryEntry> builder)
  {
    base.Configure(builder);

    builder.Property(h => h.ProjectId)
      .IsRequired();

    builder.Property(h => h.EventType)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(h => h.ActorUserId)
      .IsRequired();

    builder.Property(h => h.Summary)
      .IsRequired()
      .HasMaxLength(ProjectHistoryConstraints.SUMMARY_MAX_LENGTH);

    builder.Property(h => h.PreviousValue)
      .HasMaxLength(ProjectHistoryConstraints.VALUE_MAX_LENGTH);

    builder.Property(h => h.CurrentValue)
      .HasMaxLength(ProjectHistoryConstraints.VALUE_MAX_LENGTH);
  }
}

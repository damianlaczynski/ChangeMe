using ChangeMe.Backend.Domain.Aggregates.Project.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Projects;

public class ProjectMembershipHistoryEntryConfiguration : BaseEntityTypeConfiguration<ProjectMembershipHistoryEntry>
{
  protected override string TableName => "project_membership_history";

  public override void Configure(EntityTypeBuilder<ProjectMembershipHistoryEntry> builder)
  {
    base.Configure(builder);

    builder.Property(h => h.ProjectId)
      .IsRequired();

    builder.Property(h => h.EventType)
      .IsRequired()
      .HasConversion<string>();

    builder.Property(h => h.ActorUserId)
      .IsRequired();

    builder.Property(h => h.AffectedUserId)
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

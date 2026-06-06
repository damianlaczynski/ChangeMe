using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Aggregates.Time.Entities;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Time;

public class TimeEntryAuditLogEntryConfiguration : BaseEntityTypeConfiguration<TimeEntryAuditLogEntry>
{
  protected override string TableName => "time_entry_audit_log";

  public override void Configure(EntityTypeBuilder<TimeEntryAuditLogEntry> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.TimeEntryId).IsRequired();
    builder.Property(x => x.Operation).IsRequired();
    builder.Property(x => x.ActingUserId).IsRequired();
    builder.Property(x => x.EntryAuthorUserId).IsRequired();
    builder.Property(x => x.ProjectId).IsRequired();
    builder.Property(x => x.ProjectName).IsRequired().HasMaxLength(100);
    builder.Property(x => x.IssueTitle).HasMaxLength(200);
    builder.Property(x => x.WorkDate).IsRequired();
    builder.Property(x => x.DurationMinutes).IsRequired();
    builder.Property(x => x.Description)
      .IsRequired()
      .HasMaxLength(TimeConstraints.DescriptionMaxLength);
    builder.Property(x => x.PreviousDescription).HasMaxLength(TimeConstraints.DescriptionMaxLength);
    builder.Property(x => x.PreviousProjectName).HasMaxLength(100);
    builder.Property(x => x.PreviousIssueTitle).HasMaxLength(200);

    builder.HasIndex(x => x.CreatedAt);
    builder.HasIndex(x => x.ActingUserId);
    builder.HasIndex(x => x.EntryAuthorUserId);
    builder.HasIndex(x => x.ProjectId);
  }
}

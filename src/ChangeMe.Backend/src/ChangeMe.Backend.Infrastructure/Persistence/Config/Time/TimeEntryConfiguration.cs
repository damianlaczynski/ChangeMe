using ChangeMe.Backend.Domain.Aggregates.Time;

namespace ChangeMe.Backend.Infrastructure.Persistence.Config.Time;

public class TimeEntryConfiguration : BaseEntityTypeConfiguration<TimeEntry>
{
  protected override string TableName => "time_entries";

  public override void Configure(EntityTypeBuilder<TimeEntry> builder)
  {
    base.Configure(builder);

    builder.Property(x => x.AuthorUserId).IsRequired();
    builder.Property(x => x.ProjectId).IsRequired();
    builder.Property(x => x.WorkDate).IsRequired();
    builder.Property(x => x.DurationMinutes).IsRequired();
    builder.Property(x => x.Description)
      .IsRequired()
      .HasMaxLength(TimeConstraints.DescriptionMaxLength);

    builder.HasIndex(x => x.AuthorUserId);
    builder.HasIndex(x => x.ProjectId);
    builder.HasIndex(x => x.IssueId);
    builder.HasIndex(x => x.WorkDate);

    builder.HasOne<Domain.Aggregates.Project.Project>()
      .WithMany()
      .HasForeignKey(x => x.ProjectId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne<Domain.Aggregates.Issue.Issue>()
      .WithMany()
      .HasForeignKey(x => x.IssueId)
      .OnDelete(DeleteBehavior.SetNull);
  }
}

using ChangeMe.Backend.Domain.Aggregates.Time.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Time.Entities;

public class TimeEntryAuditLogEntry : Entity
{
  private TimeEntryAuditLogEntry() { }

  public Guid TimeEntryId { get; private set; }
  public TimeEntryAuditOperation Operation { get; private set; }
  public Guid ActingUserId { get; private set; }
  public Guid EntryAuthorUserId { get; private set; }
  public Guid ProjectId { get; private set; }
  public string ProjectName { get; private set; } = string.Empty;
  public Guid? IssueId { get; private set; }
  public string? IssueTitle { get; private set; }
  public DateOnly WorkDate { get; private set; }
  public int DurationMinutes { get; private set; }
  public string Description { get; private set; } = string.Empty;
  public DateOnly? PreviousWorkDate { get; private set; }
  public int? PreviousDurationMinutes { get; private set; }
  public string? PreviousDescription { get; private set; }
  public Guid? PreviousProjectId { get; private set; }
  public string? PreviousProjectName { get; private set; }
  public Guid? PreviousIssueId { get; private set; }
  public string? PreviousIssueTitle { get; private set; }

  public static TimeEntryAuditLogEntry ForCreate(
    Guid timeEntryId,
    Guid actingUserId,
    Guid entryAuthorUserId,
    Guid projectId,
    string projectName,
    Guid? issueId,
    string? issueTitle,
    DateOnly workDate,
    int durationMinutes,
    string description) =>
    new()
    {
      TimeEntryId = timeEntryId,
      Operation = TimeEntryAuditOperation.CREATED,
      ActingUserId = actingUserId,
      EntryAuthorUserId = entryAuthorUserId,
      ProjectId = projectId,
      ProjectName = projectName,
      IssueId = issueId,
      IssueTitle = issueTitle,
      WorkDate = workDate,
      DurationMinutes = durationMinutes,
      Description = description,
    };

  public static TimeEntryAuditLogEntry ForUpdate(
    Guid timeEntryId,
    Guid actingUserId,
    TimeEntry entry,
    string projectName,
    string? issueTitle,
    DateOnly previousWorkDate,
    int previousDurationMinutes,
    string previousDescription,
    Guid previousProjectId,
    string previousProjectName,
    Guid? previousIssueId,
    string? previousIssueTitle) =>
    new()
    {
      TimeEntryId = timeEntryId,
      Operation = TimeEntryAuditOperation.UPDATED,
      ActingUserId = actingUserId,
      EntryAuthorUserId = entry.AuthorUserId,
      ProjectId = entry.ProjectId,
      ProjectName = projectName,
      IssueId = entry.IssueId,
      IssueTitle = issueTitle,
      WorkDate = entry.WorkDate,
      DurationMinutes = entry.DurationMinutes,
      Description = entry.Description,
      PreviousWorkDate = previousWorkDate,
      PreviousDurationMinutes = previousDurationMinutes,
      PreviousDescription = previousDescription,
      PreviousProjectId = previousProjectId,
      PreviousProjectName = previousProjectName,
      PreviousIssueId = previousIssueId,
      PreviousIssueTitle = previousIssueTitle,
    };

  public static TimeEntryAuditLogEntry ForDelete(
    Guid timeEntryId,
    Guid actingUserId,
    Guid entryAuthorUserId,
    Guid projectId,
    string projectName,
    Guid? issueId,
    string? issueTitle,
    DateOnly workDate,
    int durationMinutes,
    string description) =>
    new()
    {
      TimeEntryId = timeEntryId,
      Operation = TimeEntryAuditOperation.DELETED,
      ActingUserId = actingUserId,
      EntryAuthorUserId = entryAuthorUserId,
      ProjectId = projectId,
      ProjectName = projectName,
      IssueId = issueId,
      IssueTitle = issueTitle,
      WorkDate = workDate,
      DurationMinutes = durationMinutes,
      Description = description,
      PreviousWorkDate = workDate,
      PreviousDurationMinutes = durationMinutes,
      PreviousDescription = description,
      PreviousProjectId = projectId,
      PreviousProjectName = projectName,
      PreviousIssueId = issueId,
      PreviousIssueTitle = issueTitle,
    };
}

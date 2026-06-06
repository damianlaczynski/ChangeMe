using ChangeMe.Backend.Domain.Aggregates.Time.Enums;
using ChangeMe.Backend.UseCases.Time.Enums;

namespace ChangeMe.Backend.UseCases.Time.Dtos;

public class TimeEntryDto
{
  public Guid Id { get; set; }
  public Guid AuthorUserId { get; set; }
  public string? AuthorName { get; set; }
  public Guid ProjectId { get; set; }
  public string ProjectName { get; set; } = string.Empty;
  public Guid? IssueId { get; set; }
  public string? IssueTitle { get; set; }
  public DateOnly WorkDate { get; set; }
  public int DurationMinutes { get; set; }
  public string DurationFormatted { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public bool CanEdit { get; set; }
  public bool CanDelete { get; set; }
}

public class TimeEntryListItemDto
{
  public Guid Id { get; set; }
  public Guid ProjectId { get; set; }
  public string ProjectName { get; set; } = string.Empty;
  public Guid? IssueId { get; set; }
  public string? IssueTitle { get; set; }
  public DateOnly WorkDate { get; set; }
  public int DurationMinutes { get; set; }
  public string DurationFormatted { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public bool CanViewProject { get; set; }
  public bool CanEdit { get; set; }
  public bool CanDelete { get; set; }
}

public class MyTimeEntriesResultDto
{
  public PaginationResult<TimeEntryListItemDto> Entries { get; set; } = PaginationResult<TimeEntryListItemDto>.Empty();
  public int TotalDurationMinutes { get; set; }
  public string TotalDurationFormatted { get; set; } = string.Empty;
}

public class IssueTimeEntryListItemDto
{
  public Guid Id { get; set; }
  public Guid AuthorUserId { get; set; }
  public string AuthorName { get; set; } = string.Empty;
  public DateOnly WorkDate { get; set; }
  public int DurationMinutes { get; set; }
  public string DurationFormatted { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public bool CanEdit { get; set; }
  public bool CanDelete { get; set; }
}

public class IssueTimeEntriesResultDto
{
  public int TotalDurationMinutes { get; set; }
  public string TotalDurationFormatted { get; set; } = string.Empty;
  public IReadOnlyList<string> ContributorNames { get; set; } = [];
  public PaginationResult<IssueTimeEntryListItemDto> Entries { get; set; } =
    PaginationResult<IssueTimeEntryListItemDto>.Empty();
}

public class RunningTimerStateDto
{
  public RunningTimerDto? Timer { get; set; }
}

public class RunningTimerDto
{
  public Guid? ProjectId { get; set; }
  public string? ProjectName { get; set; }
  public Guid? IssueId { get; set; }
  public string? IssueTitle { get; set; }
  public DateTime StartedAtUtc { get; set; }
  public int ElapsedMinutes { get; set; }
  public string ElapsedFormatted { get; set; } = string.Empty;
}

public class TimeSettingsDto
{
  public int BackdatingLimitDays { get; set; }
  public bool CanEdit { get; set; }
}

public class TimeReportRowDto
{
  public string Label { get; set; } = string.Empty;
  public Guid? UserId { get; set; }
  public Guid? ProjectId { get; set; }
  public Guid? IssueId { get; set; }
  public string? SecondaryLabel { get; set; }
  public int TotalDurationMinutes { get; set; }
  public string TotalDurationFormatted { get; set; } = string.Empty;
}

public class TimeReportMatrixCellDto
{
  public int TotalDurationMinutes { get; set; }
  public string TotalDurationFormatted { get; set; } = string.Empty;
}

public class TimeReportMatrixDto
{
  public IReadOnlyList<string> ColumnLabels { get; set; } = [];
  public IReadOnlyList<Guid> ColumnProjectIds { get; set; } = [];
  public IReadOnlyList<TimeReportMatrixRowDto> Rows { get; set; } = [];
  public IReadOnlyList<TimeReportMatrixCellDto> ColumnTotals { get; set; } = [];
  public TimeReportMatrixCellDto GrandTotal { get; set; } = new();
}

public class TimeReportMatrixRowDto
{
  public string UserLabel { get; set; } = string.Empty;
  public Guid UserId { get; set; }
  public IReadOnlyList<TimeReportMatrixCellDto> Cells { get; set; } = [];
  public TimeReportMatrixCellDto RowTotal { get; set; } = new();
}

public class TimeReportResultDto
{
  public TimeReportGroupingMode GroupingMode { get; set; }
  public int TotalDurationMinutes { get; set; }
  public string TotalDurationFormatted { get; set; } = string.Empty;
  public IReadOnlyList<TimeReportRowDto> Rows { get; set; } = [];
  public TimeReportMatrixDto? Matrix { get; set; }
}

public class ReportPersonEntryDto
{
  public DateOnly WorkDate { get; set; }
  public string ProjectName { get; set; } = string.Empty;
  public Guid? IssueId { get; set; }
  public string? IssueTitle { get; set; }
  public int DurationMinutes { get; set; }
  public string DurationFormatted { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
}

public class ExportTimeReportsResultDto
{
  public byte[] Content { get; set; } = [];
  public string FileName { get; set; } = string.Empty;
}

public class TimeEntryAuditLogEntryDto
{
  public Guid Id { get; set; }
  public Guid TimeEntryId { get; set; }
  public TimeEntryAuditOperation Operation { get; set; }
  public Guid ActingUserId { get; set; }
  public string? ActingUserName { get; set; }
  public Guid EntryAuthorUserId { get; set; }
  public string? EntryAuthorName { get; set; }
  public Guid ProjectId { get; set; }
  public string ProjectName { get; set; } = string.Empty;
  public Guid? IssueId { get; set; }
  public string? IssueTitle { get; set; }
  public DateOnly WorkDate { get; set; }
  public int DurationMinutes { get; set; }
  public string DurationFormatted { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateOnly? PreviousWorkDate { get; set; }
  public int? PreviousDurationMinutes { get; set; }
  public string? PreviousDurationFormatted { get; set; }
  public string? PreviousDescription { get; set; }
  public Guid? PreviousProjectId { get; set; }
  public string? PreviousProjectName { get; set; }
  public Guid? PreviousIssueId { get; set; }
  public string? PreviousIssueTitle { get; set; }
  public DateTime OccurredAt { get; set; }
}

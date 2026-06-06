using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Enums;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public class GetTimeReportsQuery : IQuery<TimeReportResultDto>
{
  public DateOnly DateFrom { get; set; }

  public DateOnly DateTo { get; set; }

  public List<Guid>? ProjectIds { get; set; }

  public List<Guid>? UserIds { get; set; }

  public TimeReportGroupingMode GroupingMode { get; set; } = TimeReportGroupingMode.ByPerson;
}

public class GetTimeReportsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetTimeReportsQuery, TimeReportResultDto>
{
  public async Task<Result<TimeReportResultDto>> Handle(
    GetTimeReportsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeViewReports);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var dateRangeValidation = TimeUtils.ValidateDateRange(query.DateFrom, query.DateTo);
    if (!dateRangeValidation.IsSuccess)
      return dateRangeValidation.Map();

    var report = await TimeReportBuilder.BuildAsync(context, query, cancellationToken);
    return Result.Success(report);
  }
}

internal static class TimeReportBuilder
{
  internal sealed record ReportEntryRow(
    Guid AuthorUserId,
    string AuthorName,
    Guid ProjectId,
    string ProjectName,
    Guid? IssueId,
    string? IssueTitle,
    int DurationMinutes);

  public static async Task<TimeReportResultDto> BuildAsync(
    ApplicationDbContext context,
    GetTimeReportsQuery query,
    CancellationToken cancellationToken)
  {
    var entriesQuery = context.TimeEntries
      .AsNoTracking()
      .Where(e => e.WorkDate >= query.DateFrom && e.WorkDate <= query.DateTo);

    if (query.ProjectIds?.Count > 0)
      entriesQuery = entriesQuery.Where(e => query.ProjectIds.Contains(e.ProjectId));

    if (query.UserIds?.Count > 0)
      entriesQuery = entriesQuery.Where(e => query.UserIds.Contains(e.AuthorUserId));

    var rows = await entriesQuery
      .Select(e => new ReportEntryRow(
        e.AuthorUserId,
        context.Users.Where(u => u.Id == e.AuthorUserId).Select(UserDisplayFormat.DisplayLabelExpression).FirstOrDefault() ?? string.Empty,
        e.ProjectId,
        context.Projects.Where(p => p.Id == e.ProjectId).Select(p => p.Name).FirstOrDefault() ?? string.Empty,
        e.IssueId,
        e.IssueId.HasValue
          ? context.Issues.Where(i => i.Id == e.IssueId.Value).Select(i => i.Title).FirstOrDefault()
          : null,
        e.DurationMinutes))
      .ToListAsync(cancellationToken);

    var totalMinutes = rows.Sum(r => r.DurationMinutes);

    return query.GroupingMode switch
    {
      TimeReportGroupingMode.ByPerson => BuildByPerson(rows, totalMinutes, query.GroupingMode),
      TimeReportGroupingMode.ByProject => BuildByProject(rows, totalMinutes, query.GroupingMode),
      TimeReportGroupingMode.ByIssue => BuildByIssue(rows, totalMinutes, query.GroupingMode),
      TimeReportGroupingMode.PersonAndProject => BuildPersonAndProject(rows, totalMinutes, query.GroupingMode),
      TimeReportGroupingMode.Overall => BuildOverall(totalMinutes, query.GroupingMode),
      _ => BuildOverall(totalMinutes, query.GroupingMode),
    };
  }

  private static TimeReportResultDto BuildOverall(int totalMinutes, TimeReportGroupingMode mode) =>
    new()
    {
      GroupingMode = mode,
      TotalDurationMinutes = totalMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
      Rows = [],
    };

  private static TimeReportResultDto BuildByPerson(
    IReadOnlyList<ReportEntryRow> rows,
    int totalMinutes,
    TimeReportGroupingMode mode)
  {
    var reportRows = rows
      .GroupBy(r => new { r.AuthorUserId, r.AuthorName })
      .Select(g => new TimeReportRowDto
      {
        Label = g.Key.AuthorName,
        UserId = g.Key.AuthorUserId,
        TotalDurationMinutes = g.Sum(x => x.DurationMinutes),
        TotalDurationFormatted = TimeUtils.FormatDuration(g.Sum(x => x.DurationMinutes)),
      })
      .OrderBy(r => r.Label, StringComparer.OrdinalIgnoreCase)
      .ToList();

    return new TimeReportResultDto
    {
      GroupingMode = mode,
      TotalDurationMinutes = totalMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
      Rows = reportRows,
    };
  }

  private static TimeReportResultDto BuildByProject(
    IReadOnlyList<ReportEntryRow> rows,
    int totalMinutes,
    TimeReportGroupingMode mode)
  {
    var reportRows = rows
      .GroupBy(r => new { r.ProjectId, r.ProjectName })
      .Select(g => new TimeReportRowDto
      {
        Label = g.Key.ProjectName,
        ProjectId = g.Key.ProjectId,
        TotalDurationMinutes = g.Sum(x => x.DurationMinutes),
        TotalDurationFormatted = TimeUtils.FormatDuration(g.Sum(x => x.DurationMinutes)),
      })
      .OrderBy(r => r.Label, StringComparer.OrdinalIgnoreCase)
      .ToList();

    return new TimeReportResultDto
    {
      GroupingMode = mode,
      TotalDurationMinutes = totalMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
      Rows = reportRows,
    };
  }

  private static TimeReportResultDto BuildByIssue(
    IReadOnlyList<ReportEntryRow> rows,
    int totalMinutes,
    TimeReportGroupingMode mode)
  {
    var reportRows = rows
      .Where(r => r.IssueId.HasValue)
      .GroupBy(r => new { IssueId = r.IssueId!.Value, r.IssueTitle, r.ProjectName })
      .Select(g => new TimeReportRowDto
      {
        Label = g.Key.IssueTitle ?? string.Empty,
        IssueId = g.Key.IssueId,
        SecondaryLabel = g.Key.ProjectName,
        TotalDurationMinutes = g.Sum(x => x.DurationMinutes),
        TotalDurationFormatted = TimeUtils.FormatDuration(g.Sum(x => x.DurationMinutes)),
      })
      .OrderBy(r => r.Label, StringComparer.OrdinalIgnoreCase)
      .ToList();

    return new TimeReportResultDto
    {
      GroupingMode = mode,
      TotalDurationMinutes = totalMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
      Rows = reportRows,
    };
  }

  private static TimeReportResultDto BuildPersonAndProject(
    IReadOnlyList<ReportEntryRow> rows,
    int totalMinutes,
    TimeReportGroupingMode mode)
  {
    var projectColumns = rows
      .GroupBy(r => new { r.ProjectId, r.ProjectName })
      .Select(g => new { g.Key.ProjectId, g.Key.ProjectName })
      .OrderBy(p => p.ProjectName, StringComparer.OrdinalIgnoreCase)
      .ToList();

    var userRows = rows
      .GroupBy(r => new { r.AuthorUserId, r.AuthorName })
      .OrderBy(g => g.Key.AuthorName, StringComparer.OrdinalIgnoreCase)
      .ToList();

    var matrixRows = new List<TimeReportMatrixRowDto>();
    var columnTotals = projectColumns
      .Select(_ => new TimeReportMatrixCellDto())
      .ToList();

    foreach (var userGroup in userRows)
    {
      var cells = new List<TimeReportMatrixCellDto>();
      var rowTotalMinutes = 0;

      foreach (var (project, index) in projectColumns.Select((p, i) => (p, i)))
      {
        var minutes = userGroup
          .Where(r => r.ProjectId == project.ProjectId)
          .Sum(r => r.DurationMinutes);

        rowTotalMinutes += minutes;
        columnTotals[index].TotalDurationMinutes += minutes;

        cells.Add(new TimeReportMatrixCellDto
        {
          TotalDurationMinutes = minutes,
          TotalDurationFormatted = TimeUtils.FormatDuration(minutes),
        });
      }

      matrixRows.Add(new TimeReportMatrixRowDto
      {
        UserId = userGroup.Key.AuthorUserId,
        UserLabel = userGroup.Key.AuthorName,
        Cells = cells,
        RowTotal = new TimeReportMatrixCellDto
        {
          TotalDurationMinutes = rowTotalMinutes,
          TotalDurationFormatted = TimeUtils.FormatDuration(rowTotalMinutes),
        },
      });
    }

    foreach (var columnTotal in columnTotals)
      columnTotal.TotalDurationFormatted = TimeUtils.FormatDuration(columnTotal.TotalDurationMinutes);

    return new TimeReportResultDto
    {
      GroupingMode = mode,
      TotalDurationMinutes = totalMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
      Matrix = new TimeReportMatrixDto
      {
        ColumnLabels = projectColumns.Select(p => p.ProjectName).ToList(),
        ColumnProjectIds = projectColumns.Select(p => p.ProjectId).ToList(),
        Rows = matrixRows,
        ColumnTotals = columnTotals,
        GrandTotal = new TimeReportMatrixCellDto
        {
          TotalDurationMinutes = totalMinutes,
          TotalDurationFormatted = TimeUtils.FormatDuration(totalMinutes),
        },
      },
    };
  }
}

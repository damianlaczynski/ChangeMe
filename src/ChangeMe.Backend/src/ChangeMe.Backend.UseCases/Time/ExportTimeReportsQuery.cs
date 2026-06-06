using System.Globalization;
using System.Text;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Enums;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public class ExportTimeReportsQuery : IQuery<ExportTimeReportsResultDto>
{
  public DateOnly DateFrom { get; set; }

  public DateOnly DateTo { get; set; }

  public List<Guid>? ProjectIds { get; set; }

  public List<Guid>? UserIds { get; set; }

  public TimeReportGroupingMode GroupingMode { get; set; } = TimeReportGroupingMode.ByPerson;
}

public class ExportTimeReportsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<ExportTimeReportsQuery, ExportTimeReportsResultDto>
{
  public async Task<Result<ExportTimeReportsResultDto>> Handle(
    ExportTimeReportsQuery query,
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

    var reportQuery = new GetTimeReportsQuery
    {
      DateFrom = query.DateFrom,
      DateTo = query.DateTo,
      ProjectIds = query.ProjectIds,
      UserIds = query.UserIds,
      GroupingMode = query.GroupingMode,
    };

    var report = await TimeReportBuilder.BuildAsync(context, reportQuery, cancellationToken);
    var csv = BuildCsv(report);
    var fileName = $"time-report-{DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}.csv";

    return Result.Success(new ExportTimeReportsResultDto
    {
      Content = Encoding.UTF8.GetBytes(csv),
      FileName = fileName,
    });
  }

  private static string BuildCsv(TimeReportResultDto report)
  {
    var builder = new StringBuilder();

    if (report.GroupingMode == TimeReportGroupingMode.PersonAndProject && report.Matrix is not null)
    {
      builder.Append("User");
      foreach (var column in report.Matrix.ColumnLabels)
        builder.Append(',').Append(EscapeCsv(column));
      builder.Append(",Total\n");

      foreach (var row in report.Matrix.Rows)
      {
        builder.Append(EscapeCsv(row.UserLabel));
        foreach (var cell in row.Cells)
          builder.Append(',').Append(EscapeCsv(cell.TotalDurationFormatted));
        builder.Append(',').Append(EscapeCsv(row.RowTotal.TotalDurationFormatted));
        builder.Append('\n');
      }

      builder.Append("Total");
      foreach (var columnTotal in report.Matrix.ColumnTotals)
        builder.Append(',').Append(EscapeCsv(columnTotal.TotalDurationFormatted));
      builder.Append(',').Append(EscapeCsv(report.Matrix.GrandTotal.TotalDurationFormatted));
      builder.Append('\n');

      return builder.ToString();
    }

    if (report.GroupingMode == TimeReportGroupingMode.ByIssue)
    {
      builder.Append("Issue,Project,Total time\n");
      foreach (var row in report.Rows)
        builder.Append(EscapeCsv(row.Label)).Append(',').Append(EscapeCsv(row.SecondaryLabel ?? string.Empty)).Append(',').Append(EscapeCsv(row.TotalDurationFormatted)).Append('\n');
      return builder.ToString();
    }

    if (report.GroupingMode == TimeReportGroupingMode.ByProject)
    {
      builder.Append("Project,Total time\n");
      foreach (var row in report.Rows)
        builder.Append(EscapeCsv(row.Label)).Append(',').Append(EscapeCsv(row.TotalDurationFormatted)).Append('\n');
      return builder.ToString();
    }

    if (report.GroupingMode == TimeReportGroupingMode.ByPerson)
    {
      builder.Append("User,Total time\n");
      foreach (var row in report.Rows)
        builder.Append(EscapeCsv(row.Label)).Append(',').Append(EscapeCsv(row.TotalDurationFormatted)).Append('\n');
      return builder.ToString();
    }

    builder.Append("Total time\n");
    builder.Append(EscapeCsv(report.TotalDurationFormatted)).Append('\n');
    return builder.ToString();
  }

  private static string EscapeCsv(string value)
  {
    if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
      return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    return value;
  }
}

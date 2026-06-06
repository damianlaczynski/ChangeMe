using System.Text;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Enums;
using ChangeMe.Backend.UseCases.Time.Utils;
using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Time;

public class GetTimeEntryAuditLogQuery : IQuery<PaginationResult<TimeEntryAuditLogEntryDto>>
{
  public DateOnly? DateFrom { get; set; }

  public DateOnly? DateTo { get; set; }

  public Guid? ActingUserId { get; set; }

  public Guid? EntryAuthorUserId { get; set; }

  public Guid? ProjectId { get; set; }

  public List<Domain.Aggregates.Time.Enums.TimeEntryAuditOperation>? Operations { get; set; }

  [FromQuery]
  public PaginationParameters<TimeEntryAuditLogEntryDto> PaginationParameters { get; set; } =
    new PaginationParameters<TimeEntryAuditLogEntryDto>();
}

public class GetTimeEntryAuditLogHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetTimeEntryAuditLogQuery, PaginationResult<TimeEntryAuditLogEntryDto>>
{
  public async Task<Result<PaginationResult<TimeEntryAuditLogEntryDto>>> Handle(
    GetTimeEntryAuditLogQuery query,
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

    var auditQuery = context.TimeEntryAuditLog.AsNoTracking();

    if (query.DateFrom.HasValue)
    {
      var fromUtc = query.DateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
      auditQuery = auditQuery.Where(a => a.CreatedAt >= fromUtc);
    }

    if (query.DateTo.HasValue)
    {
      var toUtc = query.DateTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
      auditQuery = auditQuery.Where(a => a.CreatedAt <= toUtc);
    }

    if (query.ActingUserId.HasValue)
      auditQuery = auditQuery.Where(a => a.ActingUserId == query.ActingUserId.Value);

    if (query.EntryAuthorUserId.HasValue)
      auditQuery = auditQuery.Where(a => a.EntryAuthorUserId == query.EntryAuthorUserId.Value);

    if (query.ProjectId.HasValue)
      auditQuery = auditQuery.Where(a => a.ProjectId == query.ProjectId.Value);

    if (query.Operations?.Count > 0)
      auditQuery = auditQuery.Where(a => query.Operations.Contains(a.Operation));

    query.PaginationParameters.SortField = nameof(TimeEntryAuditLogEntryDto.OccurredAt);
    query.PaginationParameters.Ascending = false;

    var projected = auditQuery.Select(a => new TimeEntryAuditLogEntryDto
    {
      Id = a.Id,
      TimeEntryId = a.TimeEntryId,
      Operation = a.Operation,
      ActingUserId = a.ActingUserId,
      EntryAuthorUserId = a.EntryAuthorUserId,
      ProjectId = a.ProjectId,
      ProjectName = a.ProjectName,
      IssueId = a.IssueId,
      IssueTitle = a.IssueTitle,
      WorkDate = a.WorkDate,
      DurationMinutes = a.DurationMinutes,
      Description = a.Description,
      PreviousWorkDate = a.PreviousWorkDate,
      PreviousDurationMinutes = a.PreviousDurationMinutes,
      PreviousDescription = a.PreviousDescription,
      PreviousProjectId = a.PreviousProjectId,
      PreviousProjectName = a.PreviousProjectName,
      PreviousIssueId = a.PreviousIssueId,
      PreviousIssueTitle = a.PreviousIssueTitle,
      OccurredAt = a.CreatedAt,
    });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userIds = paged.Items
      .SelectMany(a => new[] { a.ActingUserId, a.EntryAuthorUserId })
      .Distinct()
      .ToList();

    var userLookup = await context.Users
      .AsNoTracking()
      .Where(u => userIds.Contains(u.Id))
      .ToDictionaryAsync(u => u.Id, u => u.DisplayLabel, cancellationToken);

    var items = paged.Items
      .Select(a =>
      {
        a.ActingUserName = userLookup.GetValueOrDefault(a.ActingUserId);
        a.EntryAuthorName = userLookup.GetValueOrDefault(a.EntryAuthorUserId);
        a.DurationFormatted = TimeUtils.FormatDuration(a.DurationMinutes);
        a.PreviousDurationFormatted = a.PreviousDurationMinutes.HasValue
          ? TimeUtils.FormatDuration(a.PreviousDurationMinutes.Value)
          : null;
        return a;
      })
      .ToList();

    return Result.Success(PaginationResult<TimeEntryAuditLogEntryDto>.Create(
      items,
      paged.TotalCount,
      paged.CurrentPage,
      paged.PageSize,
      paged.SortField,
      paged.Ascending));
  }
}

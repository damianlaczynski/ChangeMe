using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;
using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Time;

public class GetReportPersonEntriesQuery : IQuery<PaginationResult<ReportPersonEntryDto>>
{
  public Guid UserId { get; set; }

  public DateOnly DateFrom { get; set; }

  public DateOnly DateTo { get; set; }

  public List<Guid>? ProjectIds { get; set; }

  [FromQuery]
  public PaginationParameters<ReportPersonEntryDto> PaginationParameters { get; set; } = new();
}

public class GetReportPersonEntriesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetReportPersonEntriesQuery, PaginationResult<ReportPersonEntryDto>>
{
  public async Task<Result<PaginationResult<ReportPersonEntryDto>>> Handle(
    GetReportPersonEntriesQuery query,
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

    if (query.UserId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(query.UserId), "User is required."));

    var userExists = await context.Users
      .AsNoTracking()
      .AnyAsync(u => u.Id == query.UserId, cancellationToken);
    if (!userExists)
      return Result.NotFound();

    var entriesQuery = context.TimeEntries
      .AsNoTracking()
      .Where(e =>
        e.AuthorUserId == query.UserId
        && e.WorkDate >= query.DateFrom
        && e.WorkDate <= query.DateTo);

    if (query.ProjectIds?.Count > 0)
      entriesQuery = entriesQuery.Where(e => query.ProjectIds.Contains(e.ProjectId));

    query.PaginationParameters ??= new PaginationParameters<ReportPersonEntryDto>();
    query.PaginationParameters.Validate();
    query.PaginationParameters.SortField = nameof(ReportPersonEntryDto.WorkDate);
    query.PaginationParameters.Ascending = false;

    var projected = entriesQuery.Select(e => new ReportPersonEntryDto
    {
      WorkDate = e.WorkDate,
      ProjectName = context.Projects.Where(p => p.Id == e.ProjectId).Select(p => p.Name).FirstOrDefault() ?? string.Empty,
      IssueId = e.IssueId,
      IssueTitle = e.IssueId.HasValue
        ? context.Issues.Where(i => i.Id == e.IssueId.Value).Select(i => i.Title).FirstOrDefault()
        : null,
      DurationMinutes = e.DurationMinutes,
      Description = e.Description,
    });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var items = paged.Items
      .Select(item =>
      {
        item.DurationFormatted = TimeUtils.FormatDuration(item.DurationMinutes);
        return item;
      })
      .ToList();

    return Result.Success(PaginationResult<ReportPersonEntryDto>.Create(
      items,
      paged.TotalCount,
      paged.CurrentPage,
      paged.PageSize,
      paged.SortField,
      paged.Ascending));
  }
}

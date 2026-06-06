using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;
using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Time;

public class GetMyTimeEntriesQuery : IQuery<MyTimeEntriesResultDto>
{
  public DateOnly? DateFrom { get; set; }
  public DateOnly? DateTo { get; set; }
  public Guid? ProjectId { get; set; }

  [FromQuery]
  public PaginationParameters<TimeEntryListItemDto> PaginationParameters { get; set; } = new();
}

public class GetMyTimeEntriesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetMyTimeEntriesQuery, MyTimeEntriesResultDto>
{
  public async Task<Result<MyTimeEntriesResultDto>> Handle(
    GetMyTimeEntriesQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeViewOwn);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var dateRangeValidation = TimeUtils.ValidateDateRange(query.DateFrom, query.DateTo);
    if (!dateRangeValidation.IsSuccess)
      return dateRangeValidation.Map();

    var viewableProjectIds = await TimeUtils.GetViewableProjectIdsAsync(context, currentUserId, cancellationToken);

    if (query.ProjectId.HasValue && !viewableProjectIds.Contains(query.ProjectId.Value))
      return Result.Forbidden(ProjectPermissionCodes.ForbiddenMessage);

    var entriesQuery = context.TimeEntries
      .AsNoTracking()
      .Where(e => e.AuthorUserId == currentUserId);

    if (query.DateFrom.HasValue)
      entriesQuery = entriesQuery.Where(e => e.WorkDate >= query.DateFrom.Value);

    if (query.DateTo.HasValue)
      entriesQuery = entriesQuery.Where(e => e.WorkDate <= query.DateTo.Value);

    if (query.ProjectId.HasValue)
      entriesQuery = entriesQuery.Where(e => e.ProjectId == query.ProjectId.Value);
    else if (viewableProjectIds.Count > 0)
      entriesQuery = entriesQuery.Where(e => viewableProjectIds.Contains(e.ProjectId));

    var totalDurationMinutes = await entriesQuery.SumAsync(e => e.DurationMinutes, cancellationToken);

    query.PaginationParameters ??= new PaginationParameters<TimeEntryListItemDto>();
    query.PaginationParameters.Validate();
    query.PaginationParameters.SortField = nameof(TimeEntryListItemDto.WorkDate);
    query.PaginationParameters.Ascending = false;

    var projected = entriesQuery.Select(e => new TimeEntryListItemDto
    {
      Id = e.Id,
      ProjectId = e.ProjectId,
      ProjectName = context.Projects.Where(p => p.Id == e.ProjectId).Select(p => p.Name).FirstOrDefault() ?? string.Empty,
      IssueId = e.IssueId,
      IssueTitle = e.IssueId.HasValue
        ? context.Issues.Where(i => i.Id == e.IssueId.Value).Select(i => i.Title).FirstOrDefault()
        : null,
      WorkDate = e.WorkDate,
      DurationMinutes = e.DurationMinutes,
      Description = e.Description,
      CreatedAt = e.CreatedAt,
    });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var projectIds = paged.Items.Select(e => e.ProjectId).Distinct().ToList();
    var projectRoles = projectIds.Count == 0
      ? []
      : await context.ProjectMembers
        .AsNoTracking()
        .Where(m => m.UserId == currentUserId && projectIds.Contains(m.ProjectId))
        .ToDictionaryAsync(m => m.ProjectId, m => m.Role, cancellationToken);

    var items = paged.Items
      .Select(item =>
      {
        ProjectRole? role = projectRoles.TryGetValue(item.ProjectId, out var memberRole)
          ? memberRole
          : null;
        var canManage = TimeUtils.CanManageOwnEntry(userAccessor, role);
        item.DurationFormatted = TimeUtils.FormatDuration(item.DurationMinutes);
        item.CanViewProject = role.HasValue;
        item.CanEdit = canManage;
        item.CanDelete = canManage;
        return item;
      })
      .ToList();

    return Result.Success(new MyTimeEntriesResultDto
    {
      Entries = PaginationResult<TimeEntryListItemDto>.Create(
        items,
        paged.TotalCount,
        paged.CurrentPage,
        paged.PageSize,
        paged.SortField,
        paged.Ascending),
      TotalDurationMinutes = totalDurationMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalDurationMinutes),
    });
  }
}

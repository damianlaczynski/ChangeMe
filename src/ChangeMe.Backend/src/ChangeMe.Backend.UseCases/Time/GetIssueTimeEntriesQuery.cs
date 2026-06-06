using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;
using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Time;

public class GetIssueTimeEntriesQuery : IQuery<IssueTimeEntriesResultDto>
{
  public Guid IssueId { get; set; }

  [FromQuery]
  public PaginationParameters<IssueTimeEntryListItemDto> PaginationParameters { get; set; } =
    new PaginationParameters<IssueTimeEntryListItemDto>();
}

public class GetIssueTimeEntriesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetIssueTimeEntriesQuery, IssueTimeEntriesResultDto>
{
  public async Task<Result<IssueTimeEntriesResultDto>> Handle(
    GetIssueTimeEntriesQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .AsNoTracking()
      .Where(i => i.Id == query.IssueId)
      .Select(i => new { i.ProjectId })
      .FirstOrDefaultAsync(cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var viewAccess = await TimeUtils.EnsureCanViewProjectTimeAsync(
      context,
      issue.ProjectId,
      currentUserId,
      cancellationToken);
    if (!viewAccess.IsSuccess)
      return viewAccess.Map();

    var entriesQuery = context.TimeEntries
      .AsNoTracking()
      .Where(e => e.IssueId == query.IssueId);

    var totalDurationMinutes = await entriesQuery.SumAsync(e => e.DurationMinutes, cancellationToken);

    var contributorNames = await entriesQuery
      .Join(
        context.Users.AsNoTracking(),
        e => e.AuthorUserId,
        u => u.Id,
        (e, u) => u)
      .Select(UserDisplayFormat.DisplayLabelExpression)
      .Distinct()
      .OrderBy(name => name)
      .ToListAsync(cancellationToken);

    query.PaginationParameters.SortField = nameof(IssueTimeEntryListItemDto.WorkDate);
    query.PaginationParameters.Ascending = false;
    if (query.PaginationParameters.PageSize <= 0 || query.PaginationParameters.PageSize > 100)
      query.PaginationParameters.PageSize = 10;

    var projected = entriesQuery.Select(e => new IssueTimeEntryListItemDto
    {
      Id = e.Id,
      AuthorUserId = e.AuthorUserId,
      AuthorName = context.Users
        .Where(u => u.Id == e.AuthorUserId)
        .Select(UserDisplayFormat.DisplayLabelExpression)
        .FirstOrDefault() ?? string.Empty,
      WorkDate = e.WorkDate,
      DurationMinutes = e.DurationMinutes,
      Description = e.Description,
      CreatedAt = e.CreatedAt,
    });

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(
      context,
      issue.ProjectId,
      currentUserId,
      cancellationToken);

    var items = paged.Items
      .Select(item =>
      {
        item.DurationFormatted = TimeUtils.FormatDuration(item.DurationMinutes);
        var canManage = TimeUtils.CanManageEntry(
          userAccessor,
          currentUserId,
          item.AuthorUserId,
          roleResult.Value);
        item.CanEdit = canManage;
        item.CanDelete = canManage;
        return item;
      })
      .ToList();

    return Result.Success(new IssueTimeEntriesResultDto
    {
      TotalDurationMinutes = totalDurationMinutes,
      TotalDurationFormatted = TimeUtils.FormatDuration(totalDurationMinutes),
      ContributorNames = contributorNames,
      Entries = PaginationResult<IssueTimeEntryListItemDto>.Create(
        items,
        paged.TotalCount,
        paged.CurrentPage,
        paged.PageSize,
        paged.SortField,
        paged.Ascending),
    });
  }
}

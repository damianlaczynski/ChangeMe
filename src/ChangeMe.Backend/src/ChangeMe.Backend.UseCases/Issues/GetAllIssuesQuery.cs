using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Issues.Dtos;

namespace ChangeMe.Backend.UseCases.Issues;

public class GetAllIssuesQuery : PaginationQuery<IssueDto>
{
  public string? SearchText { get; set; }
  public List<IssueStatus>? Statuses { get; set; }
  public List<IssuePriority>? Priorities { get; set; }
  public Guid? AssignedToUserId { get; set; }
  public Guid? ProjectId { get; set; }
  public bool WatchedByMe { get; set; }
  public bool CreatedByMe { get; set; }
}

public class GetAllIssuesHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetAllIssuesQuery, PaginationResult<IssueDto>>
{
  public async Task<Result<PaginationResult<IssueDto>>> Handle(GetAllIssuesQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var viewableProjectIds = await context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.UserId == currentUserId)
      .Where(m =>
        m.Role == ProjectRole.OWNER
        || m.Role == ProjectRole.MEMBER
        || m.Role == ProjectRole.VIEWER)
      .Select(m => new { m.ProjectId, m.Role })
      .ToListAsync(cancellationToken);

    var projectIdsWithView = viewableProjectIds
      .Where(x => ProjectPermissionCodes.RoleHasPermission(x.Role, ProjectPermissionCodes.IssuesView))
      .Select(x => x.ProjectId)
      .ToHashSet();

    if (projectIdsWithView.Count == 0)
      return Result.Success(PaginationResult<IssueDto>.Empty());

    if (query.ProjectId.HasValue)
    {
      if (!projectIdsWithView.Contains(query.ProjectId.Value))
        return Result.Forbidden(ProjectPermissionCodes.ForbiddenMessage);
    }

    var issuesQuery = context.Issues
      .AsNoTracking()
      .Where(i => projectIdsWithView.Contains(i.ProjectId));

    if (query.ProjectId.HasValue)
      issuesQuery = issuesQuery.Where(i => i.ProjectId == query.ProjectId.Value);

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
      var parsedIssueId = Guid.TryParse(searchText, out var issueId) ? issueId : Guid.Empty;
#if PostgreSQL
      issuesQuery = issuesQuery.Where(i =>
        EF.Functions.ILike(i.Title, $"%{searchText}%")
        || EF.Functions.ILike(i.Description, $"%{searchText}%")
        || (parsedIssueId != Guid.Empty && i.Id == parsedIssueId));
#else
      issuesQuery = issuesQuery.Where(i =>
        EF.Functions.Like(i.Title, $"%{searchText}%")
        || EF.Functions.Like(i.Description, $"%{searchText}%")
        || (parsedIssueId != Guid.Empty && i.Id == parsedIssueId));
#endif
    }

    if (query.Statuses?.Count > 0)
      issuesQuery = issuesQuery.Where(i => query.Statuses.Contains(i.Status));

    if (query.Priorities?.Count > 0)
      issuesQuery = issuesQuery.Where(i => query.Priorities.Contains(i.Priority));

    if (query.AssignedToUserId.HasValue)
      issuesQuery = issuesQuery.Where(i => i.AssignedToUserId == query.AssignedToUserId.Value);

    if (query.WatchedByMe)
      issuesQuery = issuesQuery.Where(i => i.Watchers.Any(w => w.UserId == currentUserId));

    if (query.CreatedByMe)
      issuesQuery = issuesQuery.Where(i => i.CreatedBy == currentUserId || i.AssignedToUserId == currentUserId);

    var projectedIssues = issuesQuery
      .Select(i => new IssueDto
      {
        Id = i.Id,
        Title = i.Title,
        Description = i.Description,
        Status = i.Status,
        Priority = i.Priority,
        ProjectId = i.ProjectId,
        ProjectName = context.Projects
          .Where(p => p.Id == i.ProjectId)
          .Select(p => p.Name)
          .FirstOrDefault(),
        CreatedBy = i.CreatedBy,
        CreatedByName = context.Users
          .Where(u => u.Id == i.CreatedBy)
          .Select(UserDisplayFormat.DisplayLabelExpression)
          .FirstOrDefault(),
        AssignedToUserId = i.AssignedToUserId,
        AssignedToUserName = i.AssignedToUserId.HasValue
          ? context.Users
            .Where(u => u.Id == i.AssignedToUserId.Value)
            .Select(UserDisplayFormat.DisplayLabelExpression)
            .FirstOrDefault()
          : null,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
        LastActivityAt = i.LastActivityAt,
        IsWatchedByCurrentUser = i.Watchers.Any(w => w.UserId == currentUserId),
        WatchersCount = i.Watchers.Count,
        CanManage = context.ProjectMembers.Any(m =>
          m.ProjectId == i.ProjectId
          && m.UserId == currentUserId
          && (m.Role == ProjectRole.OWNER || m.Role == ProjectRole.MEMBER)),
      });

    var pagedIssues = await projectedIssues.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    return Result.Success(pagedIssues);
  }
}

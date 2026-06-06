using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public class GetProjectMembershipHistoryQuery : PaginationQuery<ProjectMembershipHistoryEntryDto>
{
  public Guid ProjectId { get; set; }
}

public class GetProjectMembershipHistoryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectMembershipHistoryQuery, PaginationResult<ProjectMembershipHistoryEntryDto>>
{
  public async Task<Result<PaginationResult<ProjectMembershipHistoryEntryDto>>> Handle(
    GetProjectMembershipHistoryQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, query.ProjectId, currentUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.MembersView);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var projected = context.ProjectMembershipHistory
      .AsNoTracking()
      .Where(h => h.ProjectId == query.ProjectId)
      .Select(h => new ProjectMembershipHistoryEntryDto
      {
        Id = h.Id,
        EventType = h.EventType,
        ActorUserId = h.ActorUserId,
        AffectedUserId = h.AffectedUserId,
        Summary = h.Summary,
        PreviousValue = h.PreviousValue,
        CurrentValue = h.CurrentValue,
        CreatedAt = h.CreatedAt,
      });

    query.PaginationParameters.SortField = nameof(ProjectMembershipHistoryEntryDto.CreatedAt);
    query.PaginationParameters.Ascending = false;

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userIds = paged.Items
      .SelectMany(h => new[] { h.ActorUserId, h.AffectedUserId })
      .Distinct()
      .Where(id => id != ProjectAuthorization.SystemActorUserId)
      .ToList();

    var userLookup = await context.Users
      .AsNoTracking()
      .Where(u => userIds.Contains(u.Id))
      .ToDictionaryAsync(u => u.Id, u => u.DisplayLabel, cancellationToken);

    var items = paged.Items
      .Select(h =>
      {
        h.ActorName = ProjectsUtils.ResolveActorName(h.ActorUserId, userLookup);
        h.AffectedUserName = userLookup.GetValueOrDefault(h.AffectedUserId);
        return h;
      })
      .ToList();

    return Result.Success(PaginationResult<ProjectMembershipHistoryEntryDto>.Create(
      items,
      paged.TotalCount,
      paged.CurrentPage,
      paged.PageSize,
      paged.SortField,
      paged.Ascending));
  }
}

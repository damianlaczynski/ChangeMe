using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public class GetProjectOperationHistoryQuery : PaginationQuery<ProjectOperationHistoryEntryDto>
{
  public Guid ProjectId { get; set; }
}

public class GetProjectOperationHistoryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectOperationHistoryQuery, PaginationResult<ProjectOperationHistoryEntryDto>>
{
  public async Task<Result<PaginationResult<ProjectOperationHistoryEntryDto>>> Handle(
    GetProjectOperationHistoryQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, query.ProjectId, currentUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.View);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var projected = context.ProjectOperationHistory
      .AsNoTracking()
      .Where(h => h.ProjectId == query.ProjectId)
      .Select(h => new ProjectOperationHistoryEntryDto
      {
        Id = h.Id,
        EventType = h.EventType,
        ActorUserId = h.ActorUserId,
        Summary = h.Summary,
        PreviousValue = h.PreviousValue,
        CurrentValue = h.CurrentValue,
        CreatedAt = h.CreatedAt,
      });

    query.PaginationParameters.SortField = nameof(ProjectOperationHistoryEntryDto.CreatedAt);
    query.PaginationParameters.Ascending = false;

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);

    var userIds = paged.Items
      .Select(h => h.ActorUserId)
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
        return h;
      })
      .ToList();

    return Result.Success(PaginationResult<ProjectOperationHistoryEntryDto>.Create(
      items,
      paged.TotalCount,
      paged.CurrentPage,
      paged.PageSize,
      paged.SortField,
      paged.Ascending));
  }
}

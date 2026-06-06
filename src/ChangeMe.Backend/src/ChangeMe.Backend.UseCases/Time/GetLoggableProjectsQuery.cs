using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record GetLoggableProjectsQuery() : IQuery<IReadOnlyList<ProjectOptionDto>>;

public class GetLoggableProjectsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetLoggableProjectsQuery, IReadOnlyList<ProjectOptionDto>>
{
  public async Task<Result<IReadOnlyList<ProjectOptionDto>>> Handle(
    GetLoggableProjectsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeLogOwn);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var allowedRoles = ProjectsUtils.GetRolesWithPermission(ProjectPermissionCodes.TimeLog);

    var projects = await context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.UserId == currentUserId && allowedRoles.Contains(m.Role))
      .Join(
        context.Projects.AsNoTracking(),
        member => member.ProjectId,
        project => project.Id,
        (member, project) => project)
      .OrderBy(p => p.Name)
      .Select(p => new ProjectOptionDto
      {
        Id = p.Id,
        Name = p.Name,
      })
      .ToListAsync(cancellationToken);

    return Result.Success((IReadOnlyList<ProjectOptionDto>)projects);
  }
}

using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record GetManageableProjectsQuery(string? PermissionCode = null) : IQuery<IReadOnlyList<ProjectOptionDto>>;

public class GetManageableProjectsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetManageableProjectsQuery, IReadOnlyList<ProjectOptionDto>>
{
  public async Task<Result<IReadOnlyList<ProjectOptionDto>>> Handle(
    GetManageableProjectsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var permissionCode = string.IsNullOrWhiteSpace(query.PermissionCode)
      ? ProjectPermissionCodes.IssuesManage
      : query.PermissionCode;
    var allowedRoles = ProjectsUtils.GetRolesWithPermission(permissionCode);

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

using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record ChangeProjectMemberRoleCommand(Guid ProjectId, Guid UserId, ProjectRole Role) : ICommand<bool>;

public class ChangeProjectMemberRoleHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<ChangeProjectMemberRoleCommand, bool>
{
  public async Task<Result<bool>> Handle(ChangeProjectMemberRoleCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, command.ProjectId, actorUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.MembersManage);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var project = await context.Projects
      .Include(p => p.Members)
      .Include(p => p.MembershipHistory)
      .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

    if (project is null)
      return Result.NotFound();

    var historyBefore = project.MembershipHistory.Count;
    var changeResult = project.ChangeMemberRole(command.UserId, command.Role, actorUserId);
    if (!changeResult.IsSuccess)
      return changeResult.Map();

    var newHistory = project.MembershipHistory.Skip(historyBefore).ToList();
    await context.ProjectMembershipHistory.AddRangeAsync(newHistory, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}

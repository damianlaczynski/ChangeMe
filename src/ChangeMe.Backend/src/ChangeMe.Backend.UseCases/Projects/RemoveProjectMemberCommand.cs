using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : ICommand<bool>;

public class RemoveProjectMemberHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<RemoveProjectMemberCommand, bool>
{
  public async Task<Result<bool>> Handle(RemoveProjectMemberCommand command, CancellationToken cancellationToken)
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

    var member = project.Members.FirstOrDefault(m => m.UserId == command.UserId);
    if (member is null)
      return Result.NotFound();

    var historyBefore = project.MembershipHistory.Count;
    var removeResult = project.RemoveMember(command.UserId, actorUserId);
    if (!removeResult.IsSuccess)
      return removeResult.Map();

    context.ProjectMembers.Remove(member);
    var newHistory = project.MembershipHistory.Skip(historyBefore).ToList();
    await context.ProjectMembershipHistory.AddRangeAsync(newHistory, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}

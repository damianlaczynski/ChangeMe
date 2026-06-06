using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record DeleteProjectCommand(Guid Id) : ICommand<bool>;

public class DeleteProjectHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeleteProjectCommand, bool>
{
  public async Task<Result<bool>> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, command.Id, actorUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.Manage);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var loadResult = await ProjectsUtils.LoadProjectAsync(context, command.Id, includeMembers: false, cancellationToken);
    if (!loadResult.IsSuccess)
      return loadResult.Map();

    var project = loadResult.Value;
    if (project.IsSystem)
      return Result.Forbidden("System projects cannot be modified.");

    var hasIssues = await context.Issues.AnyAsync(i => i.ProjectId == project.Id, cancellationToken);
    if (hasIssues)
      return Result.Conflict(ProjectConstraints.HasIssuesDeleteMessage);

    context.Projects.Remove(project);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}

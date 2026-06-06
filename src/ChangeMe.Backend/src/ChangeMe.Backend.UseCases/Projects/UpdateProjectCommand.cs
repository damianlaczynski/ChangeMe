using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record UpdateProjectCommand(Guid Id, string Name, string? Description) : ICommand<ProjectDetailsDto>;

public class UpdateProjectHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateProjectCommand, ProjectDetailsDto>
{
  public async Task<Result<ProjectDetailsDto>> Handle(
    UpdateProjectCommand command,
    CancellationToken cancellationToken)
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

    var project = await context.Projects
      .Include(p => p.OperationHistory)
      .FirstAsync(p => p.Id == command.Id, cancellationToken);
    if (project.IsSystem)
      return Result.Forbidden("System projects cannot be modified.");

    var uniqueNameResult = await ProjectsUtils.EnsureUniqueProjectNameAsync(
      context,
      command.Name,
      command.Id,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var operationHistoryBefore = project.OperationHistory.Count;
    var updateResult = project.Update(command.Name, command.Description, actorUserId);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    var newOperationHistory = project.OperationHistory.Skip(operationHistoryBefore).ToList();
    if (newOperationHistory.Count > 0)
      await context.ProjectOperationHistory.AddRangeAsync(newOperationHistory, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetProjectByIdQuery(project.Id), cancellationToken);
  }
}

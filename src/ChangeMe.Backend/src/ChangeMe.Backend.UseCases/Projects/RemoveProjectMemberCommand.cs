using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record RemoveProjectMemberCommand(
  Guid ProjectId,
  Guid UserId) : ICommand<ProjectDetailsDto>;

public class RemoveProjectMemberHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<RemoveProjectMemberCommand, ProjectDetailsDto>
{
  public async ValueTask<Result<ProjectDetailsDto>> Handle(
    RemoveProjectMemberCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var project = await context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

    if (project is null)
      return Result.NotFound();

    var manageResult = ProjectsUtils.EnsureCanManageProject(project, currentUserId);
    if (!manageResult.IsSuccess)
      return manageResult.Map();

    var removeResult = project.RemoveMember(command.UserId);
    if (!removeResult.IsSuccess)
      return removeResult.Map();

    context.ProjectMembers.Remove(removeResult.Value);
    await context.SaveChangesAsync(cancellationToken);

    var updatedProjectResult = await mediator.Send(new GetProjectByIdQuery { Id = project.Id }, cancellationToken);
    if (!updatedProjectResult.IsSuccess)
      return updatedProjectResult.Map();

    return updatedProjectResult;
  }
}

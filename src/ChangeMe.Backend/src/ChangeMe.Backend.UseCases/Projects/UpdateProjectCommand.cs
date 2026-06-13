using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record UpdateProjectCommand(
  Guid Id,
  string Name,
  string Key,
  string? Description,
  ProjectVisibility Visibility,
  ProjectStatus Status,
  string? Color) : ICommand<ProjectDetailsDto>;

public class UpdateProjectHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateProjectCommand, ProjectDetailsDto>
{
  public async ValueTask<Result<ProjectDetailsDto>> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var project = await context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

    if (project is null)
      return Result.NotFound();

    var manageResult = ProjectsUtils.EnsureCanManageProject(project, currentUserId);
    if (!manageResult.IsSuccess)
      return manageResult.Map();

    if (await ProjectsUtils.IsKeyTakenAsync(context, command.Key, command.Id, cancellationToken))
      return Result<ProjectDetailsDto>.Conflict(ProjectsUtils.DuplicateKeyMessage);

    var updateResult = project.UpdateProfile(
      command.Name,
      command.Key,
      command.Description,
      command.Visibility,
      command.Status,
      command.Color);

    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var updatedProjectResult = await mediator.Send(new GetProjectByIdQuery { Id = project.Id }, cancellationToken);
    if (!updatedProjectResult.IsSuccess)
      return updatedProjectResult.Map();

    return updatedProjectResult;
  }
}

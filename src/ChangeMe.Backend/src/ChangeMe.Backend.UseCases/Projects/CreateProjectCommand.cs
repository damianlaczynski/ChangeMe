using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record CreateProjectCommand(
  string Name,
  string Key,
  string? Description,
  ProjectVisibility Visibility,
  string? Color) : ICommand<ProjectDetailsDto>;

public class CreateProjectHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateProjectCommand, ProjectDetailsDto>
{
  public async ValueTask<Result<ProjectDetailsDto>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    if (await ProjectsUtils.IsKeyTakenAsync(context, command.Key, null, cancellationToken))
      return Result<ProjectDetailsDto>.Conflict(ProjectsUtils.DuplicateKeyMessage);

    var createResult = Project.Create(
      command.Name,
      command.Key,
      command.Description,
      command.Visibility,
      command.Color);

    if (!createResult.IsSuccess)
      return createResult.Map();

    var project = createResult.Value;
    var ownerResult = project.AddMember(actorUserId, ProjectMemberRole.OWNER);
    if (!ownerResult.IsSuccess)
      return ownerResult.Map();

    await context.Projects.AddAsync(project, cancellationToken);
    await context.ProjectMembers.AddAsync(ownerResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var createdProjectResult = await mediator.Send(new GetProjectByIdQuery { Id = project.Id }, cancellationToken);
    if (!createdProjectResult.IsSuccess)
      return createdProjectResult.Map();

    return Result.Created(createdProjectResult.Value, $"/projects/{createdProjectResult.Value.Id}");
  }
}

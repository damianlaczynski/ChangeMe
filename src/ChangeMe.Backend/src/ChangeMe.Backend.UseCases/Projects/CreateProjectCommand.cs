using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record CreateProjectCommand(string Name, string? Description) : ICommand<ProjectDetailsDto>;

public class CreateProjectHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateProjectCommand, ProjectDetailsDto>
{
  public async Task<Result<ProjectDetailsDto>> Handle(
    CreateProjectCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid creatorUserId)
      return Result.Unauthorized();

    var uniqueNameResult = await ProjectsUtils.EnsureUniqueProjectNameAsync(
      context,
      command.Name,
      excludeProjectId: null,
      cancellationToken);
    if (!uniqueNameResult.IsSuccess)
      return uniqueNameResult.Map();

    var projectResult = Project.Create(command.Name, command.Description, creatorUserId);
    if (!projectResult.IsSuccess)
      return projectResult.Map();

    var project = projectResult.Value;
    await context.Projects.AddAsync(project, cancellationToken);
    await context.ProjectMembers.AddRangeAsync(project.Members, cancellationToken);
    await context.ProjectMembershipHistory.AddRangeAsync(project.MembershipHistory, cancellationToken);
    await context.ProjectOperationHistory.AddRangeAsync(project.OperationHistory, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var detailsResult = await mediator.Send(new GetProjectByIdQuery(project.Id), cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    return Result.Created(detailsResult.Value, $"/projects/{project.Id}");
  }
}

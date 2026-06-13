using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record AddProjectMemberCommand(
  Guid ProjectId,
  Guid UserId,
  ProjectMemberRole Role) : ICommand<ProjectDetailsDto>;

public class AddProjectMemberHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<AddProjectMemberCommand, ProjectDetailsDto>
{
  public async ValueTask<Result<ProjectDetailsDto>> Handle(
    AddProjectMemberCommand command,
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

    var user = await context.Users
      .AsNoTracking()
      .SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

    if (user is null)
      return Result.Invalid([new ValidationError(nameof(command.UserId), "user does not exist")]);

    if (user.Deactivated)
      return Result.Invalid([new ValidationError(nameof(command.UserId), "user is deactivated")]);

    var memberResult = project.AddMember(command.UserId, command.Role);
    if (!memberResult.IsSuccess)
      return memberResult.Map();

    await context.ProjectMembers.AddAsync(memberResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var updatedProjectResult = await mediator.Send(new GetProjectByIdQuery { Id = project.Id }, cancellationToken);
    if (!updatedProjectResult.IsSuccess)
      return updatedProjectResult.Map();

    return updatedProjectResult;
  }
}

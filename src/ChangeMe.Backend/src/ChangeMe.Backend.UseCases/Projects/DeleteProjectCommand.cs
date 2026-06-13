using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record DeleteProjectCommand(Guid Id) : ICommand<bool>;

public class DeleteProjectHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeleteProjectCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
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

    var hasIssues = await context.Issues.AnyAsync(i => i.ProjectId == project.Id, cancellationToken);
    if (hasIssues)
      return Result<bool>.Error(ProjectsUtils.ProjectHasIssuesMessage);

    context.Projects.Remove(project);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}

using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed class GetProjectByIdQuery : IQuery<ProjectDetailsDto>
{
  public Guid Id { get; set; }
}

public class GetProjectByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectByIdQuery, ProjectDetailsDto>
{
  public async ValueTask<Result<ProjectDetailsDto>> Handle(
    GetProjectByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var projectResult = await ProjectsUtils.GetAccessibleProjectReadOnlyAsync(
      context,
      query.Id,
      currentUserId,
      cancellationToken);

    if (!projectResult.IsSuccess)
      return projectResult.Map();

    var project = projectResult.Value;
    var issueCount = await context.Issues.CountAsync(i => i.ProjectId == project.Id, cancellationToken);
    var userLookup = await ProjectsUtils.GetUserDisplayNameLookupAsync(
      context,
      project.Members.Select(m => m.UserId),
      cancellationToken);

    return Result.Success(ProjectsUtils.ToDetailsDto(
      project,
      issueCount,
      project.Members.Count,
      userLookup,
      currentUserId));
  }
}

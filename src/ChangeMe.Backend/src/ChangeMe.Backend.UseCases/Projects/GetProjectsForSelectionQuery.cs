using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record GetProjectsForSelectionQuery : IQuery<IReadOnlyList<ProjectSelectionItemDto>>;

public class GetProjectsForSelectionHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectsForSelectionQuery, IReadOnlyList<ProjectSelectionItemDto>>
{
  public async ValueTask<Result<IReadOnlyList<ProjectSelectionItemDto>>> Handle(
    GetProjectsForSelectionQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var projects = await ProjectsUtils.ApplyVisibilityFilter(
        context.Projects.AsNoTracking(),
        currentUserId)
      .OrderBy(p => p.Name)
      .Select(p => new ProjectSelectionItemDto
      {
        Id = p.Id,
        Name = p.Name,
        Key = p.Key,
        Color = p.Color,
        Status = p.Status
      })
      .ToListAsync(cancellationToken);

    return Result.Success((IReadOnlyList<ProjectSelectionItemDto>)projects);
  }
}

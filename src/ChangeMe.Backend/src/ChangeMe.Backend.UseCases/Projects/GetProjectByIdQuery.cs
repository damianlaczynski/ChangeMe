using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public record GetProjectByIdQuery(Guid Id) : IQuery<ProjectDetailsDto>;

public class GetProjectByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectByIdQuery, ProjectDetailsDto>
{
  public async Task<Result<ProjectDetailsDto>> Handle(
    GetProjectByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, query.Id, currentUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.View);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var project = await context.Projects
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

    if (project is null)
      return Result.NotFound();

    var issueCount = await context.Issues.CountAsync(i => i.ProjectId == project.Id, cancellationToken);

    var details = ProjectsUtils.ToDetailsDto(project, roleResult.Value, issueCount);

    if (details.CanViewLoggedTime)
    {
      details.LoggedTimeMinutes = await context.TimeEntries
        .AsNoTracking()
        .Where(e => e.ProjectId == project.Id)
        .SumAsync(e => e.DurationMinutes, cancellationToken);
      details.LoggedTimeFormatted = TimeUtils.FormatDuration(details.LoggedTimeMinutes);
    }

    return Result.Success(details);
  }
}

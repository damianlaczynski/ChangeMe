using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record GetIssueByIdQuery(Guid Id) : IQuery<IssueDetailsDto>;

public class GetIssueByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetIssueByIdQuery, IssueDetailsDto>
{
  public async Task<Result<IssueDetailsDto>> Handle(GetIssueByIdQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .AsNoTracking()
      .Include(i => i.AcceptanceCriteria)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var accessResult = await IssuesUtils.ValidateProjectIssueAccessAsync(
      context,
      issue.ProjectId,
      currentUserId,
      ProjectPermissionCodes.IssuesView,
      cancellationToken);
    if (!accessResult.IsSuccess)
      return accessResult.Map();

    var roleResult = await IssuesUtils.GetProjectMemberRoleAsync(
      context,
      issue.ProjectId,
      currentUserId,
      cancellationToken);

    var projectName = await context.Projects
      .AsNoTracking()
      .Where(p => p.Id == issue.ProjectId)
      .Select(p => p.Name)
      .FirstOrDefaultAsync(cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      IssuesUtils.CollectRelatedUserIds(issue),
      cancellationToken);

    var canViewProject = ProjectAuthorization.HasPermission(roleResult.Value, ProjectPermissionCodes.View);
    var canManage = ProjectAuthorization.HasPermission(roleResult.Value, ProjectPermissionCodes.IssuesManage);

    return Result.Success(issue.ToDetailsDto(
      userLookup,
      userAccessor.UserId,
      projectName,
      canViewProject,
      canManage));
  }
}

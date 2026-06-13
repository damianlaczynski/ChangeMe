using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed record GetProjectOverviewQuery(Guid Id) : IQuery<ProjectOverviewDto>;

public class GetProjectOverviewHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectOverviewQuery, ProjectOverviewDto>
{
  public async ValueTask<Result<ProjectOverviewDto>> Handle(
    GetProjectOverviewQuery query,
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
    var issues = await context.Issues
      .AsNoTracking()
      .Where(i => i.ProjectId == project.Id)
      .Select(i => i.Status)
      .ToListAsync(cancellationToken);

    return Result.Success(new ProjectOverviewDto
    {
      Id = project.Id,
      Name = project.Name,
      Key = project.Key,
      Description = project.Description,
      Status = project.Status,
      Visibility = project.Visibility,
      Color = project.Color,
      TotalIssues = issues.Count,
      NewIssues = issues.Count(s => s == IssueStatus.NEW),
      InProgressIssues = issues.Count(s => s == IssueStatus.IN_PROGRESS),
      ResolvedIssues = issues.Count(s => s == IssueStatus.RESOLVED),
      ClosedIssues = issues.Count(s => s == IssueStatus.CLOSED),
      MemberCount = project.Members.Count
    });
  }
}

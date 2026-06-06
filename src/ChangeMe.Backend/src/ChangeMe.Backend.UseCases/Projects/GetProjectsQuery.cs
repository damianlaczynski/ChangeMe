using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public class GetProjectsQuery : PaginationQuery<ProjectListItemDto>
{
  public string? SearchText { get; set; }
}

public class GetProjectsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectsQuery, PaginationResult<ProjectListItemDto>>
{
  public async Task<Result<PaginationResult<ProjectListItemDto>>> Handle(
    GetProjectsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var projectsQuery = context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.UserId == currentUserId)
      .Join(
        context.Projects.AsNoTracking(),
        member => member.ProjectId,
        project => project.Id,
        (member, project) => new { member, project })
      .Select(x => x.project);

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      projectsQuery = projectsQuery.Where(p =>
        EF.Functions.ILike(p.Name, $"%{searchText}%")
        || EF.Functions.ILike(p.Description, $"%{searchText}%"));
#else
      projectsQuery = projectsQuery.Where(p =>
        EF.Functions.Like(p.Name, $"%{searchText}%")
        || EF.Functions.Like(p.Description, $"%{searchText}%"));
#endif
    }

    var projected = projectsQuery.Select(p => new ProjectListItemDto
    {
      Id = p.Id,
      Name = p.Name,
      Description = p.Description,
      IssueCount = context.Issues.Count(i => i.ProjectId == p.Id),
      CurrentUserRole = context.ProjectMembers
        .Where(m => m.ProjectId == p.Id && m.UserId == currentUserId)
        .Select(m => m.Role)
        .First(),
      IsSystem = p.IsSystem,
      CanManage = context.ProjectMembers.Any(m =>
        m.ProjectId == p.Id
        && m.UserId == currentUserId
        && m.Role == ProjectRole.OWNER),
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedProjects = await projected.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedProjects);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "Issues" => nameof(ProjectListItemDto.IssueCount),
      "Name" or _ => nameof(ProjectListItemDto.Name),
    };
}

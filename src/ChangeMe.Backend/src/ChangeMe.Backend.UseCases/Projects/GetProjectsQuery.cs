using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public sealed class GetProjectsQuery : PaginationQuery<ProjectListItemDto>
{
  public string? SearchText { get; set; }
  public List<ProjectStatus>? Statuses { get; set; }
}

public class GetProjectsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectsQuery, PaginationResult<ProjectListItemDto>>
{
  public async ValueTask<Result<PaginationResult<ProjectListItemDto>>> Handle(
    GetProjectsQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var projectsQuery = ProjectsUtils.ApplyVisibilityFilter(
      context.Projects.AsNoTracking(),
      currentUserId);

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      projectsQuery = projectsQuery.Where(p =>
        EF.Functions.ILike(p.Name, $"%{searchText}%")
        || EF.Functions.ILike(p.Key, $"%{searchText}%")
        || (p.Description != null && EF.Functions.ILike(p.Description, $"%{searchText}%")));
#else
      projectsQuery = projectsQuery.Where(p =>
        EF.Functions.Like(p.Name, $"%{searchText}%")
        || EF.Functions.Like(p.Key, $"%{searchText}%")
        || (p.Description != null && EF.Functions.Like(p.Description, $"%{searchText}%")));
#endif
    }

    if (query.Statuses?.Count > 0)
      projectsQuery = projectsQuery.Where(p => query.Statuses.Contains(p.Status));

    var projected = projectsQuery.Select(p => new ProjectListItemDto
    {
      Id = p.Id,
      Name = p.Name,
      Key = p.Key,
      Description = p.Description,
      Status = p.Status,
      Visibility = p.Visibility,
      Color = p.Color,
      IssueCount = context.Issues.Count(i => i.ProjectId == p.Id),
      MemberCount = p.Members.Count,
      CurrentUserRole = p.Members
        .Where(m => m.UserId == currentUserId)
        .Select(m => (ProjectMemberRole?)m.Role)
        .FirstOrDefault()
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
      "Key" => nameof(ProjectListItemDto.Key),
      "Issues" => nameof(ProjectListItemDto.IssueCount),
      "Members" => nameof(ProjectListItemDto.MemberCount),
      "Status" => nameof(ProjectListItemDto.Status),
      "Name" or _ => nameof(ProjectListItemDto.Name)
    };
}

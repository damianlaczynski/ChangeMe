using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;
using ChangeMe.Backend.UseCases.Projects.Utils;

namespace ChangeMe.Backend.UseCases.Projects;

public class GetProjectMembersQuery : PaginationQuery<ProjectMemberDto>
{
  public Guid ProjectId { get; set; }
  public string? SearchText { get; set; }
}

public class GetProjectMembersHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetProjectMembersQuery, PaginationResult<ProjectMemberDto>>
{
  public async Task<Result<PaginationResult<ProjectMemberDto>>> Handle(
    GetProjectMembersQuery query,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid currentUserId)
      return Result.Unauthorized();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, query.ProjectId, currentUserId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    var permissionResult = ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.MembersView);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var membersQuery = context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.ProjectId == query.ProjectId)
      .Join(
        context.Users.AsNoTracking(),
        member => member.UserId,
        user => user.Id,
        (member, user) => new { member, user });

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      membersQuery = membersQuery.Where(x =>
        EF.Functions.ILike(x.user.FirstName ?? string.Empty, $"%{searchText}%")
        || EF.Functions.ILike(x.user.LastName ?? string.Empty, $"%{searchText}%")
        || EF.Functions.ILike(x.user.Email, $"%{searchText}%"));
#else
      membersQuery = membersQuery.Where(x =>
        EF.Functions.Like(x.user.FirstName ?? string.Empty, $"%{searchText}%")
        || EF.Functions.Like(x.user.LastName ?? string.Empty, $"%{searchText}%")
        || EF.Functions.Like(x.user.Email, $"%{searchText}%"));
#endif
    }

    var canViewUsers = userAccessor.HasPermission(PermissionCodes.UsersView);

    var projected = membersQuery.Select(x => new ProjectMemberDto
    {
      UserId = x.user.Id,
      FirstName = x.user.FirstName,
      LastName = x.user.LastName,
      Email = x.user.Email,
      Role = x.member.Role,
      Deactivated = x.user.Deactivated,
      CanViewUserDetails = canViewUsers,
    });

    query.PaginationParameters.SortField = nameof(ProjectMemberDto.FirstName);

    var pagedMembers = await projected.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedMembers);
  }
}

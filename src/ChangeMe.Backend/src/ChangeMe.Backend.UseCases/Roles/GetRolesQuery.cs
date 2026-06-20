using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRolesQuery : PaginationQuery<RoleListItemDto>
{
  public string? SearchText { get; set; }
}

public class GetRolesHandler(ApplicationDbContext context)
  : IQueryHandler<GetRolesQuery, PaginationResult<RoleListItemDto>>
{
  public async ValueTask<Result<PaginationResult<RoleListItemDto>>> Handle(
    GetRolesQuery query,
    CancellationToken cancellationToken)
  {
    var rolesQuery = context.Roles.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
      rolesQuery = rolesQuery.Where(r =>
        EF.Functions.ILike(r.Name, $"%{searchText}%")
        || (r.Description != null && EF.Functions.ILike(r.Description, $"%{searchText}%")));
    }

    var projected = rolesQuery.Select(r => new RoleListItemDto
    {
      Id = r.Id,
      Name = r.Name,
      Description = r.Description,
      PermissionCount = r.Permissions.Count,
      UserCount = context.Users.Count(u => u.Roles.Any(ur => ur.RoleId == r.Id)),
      IsSystem = r.IsSystem
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedRoles = await projected.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedRoles);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "Users" => nameof(RoleListItemDto.UserCount),
      "Permissions" => nameof(RoleListItemDto.PermissionCount),
      "Name" or _ => nameof(RoleListItemDto.Name)
    };
}

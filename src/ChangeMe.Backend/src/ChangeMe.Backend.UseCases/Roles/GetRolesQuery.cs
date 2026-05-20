using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRolesQuery : IQuery<IReadOnlyList<RoleListItemDto>>
{
  public string? SearchText { get; set; }
  public string SortField { get; set; } = "Name";
  public bool Ascending { get; set; } = true;
}

public class GetRolesHandler(ApplicationDbContext context) : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
  public async Task<Result<IReadOnlyList<RoleListItemDto>>> Handle(
    GetRolesQuery query,
    CancellationToken cancellationToken)
  {
    var rolesQuery = context.Roles.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      rolesQuery = rolesQuery.Where(r =>
        EF.Functions.ILike(r.Name, $"%{searchText}%")
        || (r.Description != null && EF.Functions.ILike(r.Description, $"%{searchText}%")));
#else
      rolesQuery = rolesQuery.Where(r =>
        EF.Functions.Like(r.Name, $"%{searchText}%")
        || (r.Description != null && EF.Functions.Like(r.Description, $"%{searchText}%")));
#endif
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

    projected = ApplySort(projected, query.SortField, query.Ascending);

    var roles = await projected.ToListAsync(cancellationToken);
    return Result.Success<IReadOnlyList<RoleListItemDto>>(roles);
  }

  private static IQueryable<RoleListItemDto> ApplySort(
    IQueryable<RoleListItemDto> query,
    string sortField,
    bool ascending) =>
    (sortField, ascending) switch
    {
      ("Users", true) => query.OrderBy(x => x.UserCount).ThenBy(x => x.Name),
      ("Users", false) => query.OrderByDescending(x => x.UserCount).ThenBy(x => x.Name),
      ("Permissions", true) => query.OrderBy(x => x.PermissionCount).ThenBy(x => x.Name),
      ("Permissions", false) => query.OrderByDescending(x => x.PermissionCount).ThenBy(x => x.Name),
      (_, false) => query.OrderByDescending(x => x.Name),
      _ => query.OrderBy(x => x.Name)
    };
}

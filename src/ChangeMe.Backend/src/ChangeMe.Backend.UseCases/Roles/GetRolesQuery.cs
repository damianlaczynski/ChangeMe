using ChangeMe.Backend.UseCases.Roles.Dtos;
using QueryGrid.Abstractions;
using QueryGrid.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRolesQuery : IQuery<GridResult<RoleListItemDto>>
{
  public GridQuery Grid { get; set; } = new();
}

public class GetRolesHandler(ApplicationDbContext context)
  : IQueryHandler<GetRolesQuery, GridResult<RoleListItemDto>>
{
  public async ValueTask<Result<GridResult<RoleListItemDto>>> Handle(
    GetRolesQuery query,
    CancellationToken cancellationToken)
  {
    var rolesQuery = context.Roles.AsNoTracking();
    var gridQuery = query.Grid;

    if (!string.IsNullOrWhiteSpace(gridQuery.Search))
    {
      var searchText = gridQuery.Search.Trim().ToLowerInvariant();
      rolesQuery = rolesQuery.Where(r =>
        r.Name.ToLower().Contains(searchText)
        || (r.Description != null && r.Description.ToLower().Contains(searchText)));

      gridQuery = new GridQuery
      {
        Skip = gridQuery.Skip,
        Take = gridQuery.Take,
        Sort = gridQuery.Sort,
        Filter = gridQuery.Filter,
      };
    }

    var projected = rolesQuery
      .Select(r => new RoleListItemDto
      {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        PermissionCount = r.Permissions.Count,
        UserCount = context.Users.Count(u => u.Roles.Any(ur => ur.RoleId == r.Id)),
        IsSystem = r.IsSystem
      });

    var grid = await projected.ToGridResultAsync(gridQuery, cancellationToken: cancellationToken);
    return Result.Success(grid);
  }
}

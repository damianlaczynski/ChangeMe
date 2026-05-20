using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRoleAssignedUsersQuery : IQuery<IReadOnlyList<RoleAssignedUserDto>>
{
  public Guid RoleId { get; set; }
  public string? SearchText { get; set; }
}

public class GetRoleAssignedUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetRoleAssignedUsersQuery, IReadOnlyList<RoleAssignedUserDto>>
{
  public async Task<Result<IReadOnlyList<RoleAssignedUserDto>>> Handle(
    GetRoleAssignedUsersQuery query,
    CancellationToken cancellationToken)
  {
    var roleExists = await context.Roles.AsNoTracking().AnyAsync(x => x.Id == query.RoleId, cancellationToken);
    if (!roleExists)
      return Result<IReadOnlyList<RoleAssignedUserDto>>.NotFound();

    var usersQuery = context.Users
      .AsNoTracking()
      .Where(u => u.Roles.Any(ur => ur.RoleId == query.RoleId));

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      usersQuery = usersQuery.Where(u =>
        EF.Functions.ILike(u.FirstName, $"%{searchText}%")
        || EF.Functions.ILike(u.LastName, $"%{searchText}%")
        || EF.Functions.ILike(u.Email, $"%{searchText}%"));
#else
      usersQuery = usersQuery.Where(u =>
        EF.Functions.Like(u.FirstName, $"%{searchText}%")
        || EF.Functions.Like(u.LastName, $"%{searchText}%")
        || EF.Functions.Like(u.Email, $"%{searchText}%"));
#endif
    }

    var users = await usersQuery
      .OrderBy(u => u.LastName)
      .ThenBy(u => u.FirstName)
      .Select(u => new RoleAssignedUserDto
      {
        Id = u.Id,
        FullName = u.FirstName + " " + u.LastName,
        Email = u.Email,
        Status = u.Status
      })
      .ToListAsync(cancellationToken);

    return Result.Success<IReadOnlyList<RoleAssignedUserDto>>(users);
  }
}

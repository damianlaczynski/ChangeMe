using ChangeMe.Backend.UseCases.Roles.Dtos;

namespace ChangeMe.Backend.UseCases.Roles;

public sealed class GetRoleAssignedUsersQuery : PaginationQuery<RoleAssignedUserDto>
{
  public Guid RoleId { get; set; }
  public string? SearchText { get; set; }
}

public class GetRoleAssignedUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetRoleAssignedUsersQuery, PaginationResult<RoleAssignedUserDto>>
{
  public async ValueTask<Result<PaginationResult<RoleAssignedUserDto>>> Handle(
    GetRoleAssignedUsersQuery query,
    CancellationToken cancellationToken)
  {
    var roleExists = await context.Roles.AsNoTracking().AnyAsync(x => x.Id == query.RoleId, cancellationToken);
    if (!roleExists)
      return Result<PaginationResult<RoleAssignedUserDto>>.NotFound();

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

    var projected = usersQuery.Select(u => new RoleAssignedUserDto
    {
      Id = u.Id,
      FirstName = u.FirstName,
      LastName = u.LastName,
      Email = u.Email,
      Deactivated = u.Deactivated
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedUsers = await projected.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedUsers);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "Name" or "DisplayName" or "FullName" or "LastName" => nameof(RoleAssignedUserDto.LastName),
      "FirstName" => nameof(RoleAssignedUserDto.FirstName),
      "Email" => nameof(RoleAssignedUserDto.Email),
      "Deactivated" => nameof(RoleAssignedUserDto.Deactivated),
      _ => nameof(RoleAssignedUserDto.LastName)
    };
}

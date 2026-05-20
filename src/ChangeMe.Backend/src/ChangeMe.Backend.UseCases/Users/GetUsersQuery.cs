using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Common;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public class GetUsersQuery : PaginationQuery<UserListItemDto>
{
  public string? SearchText { get; set; }
  public List<UserStatus>? Statuses { get; set; }
}

public class GetUsersHandler(ApplicationDbContext context)
  : IQueryHandler<GetUsersQuery, PaginationResult<UserListItemDto>>
{
  public async Task<Result<PaginationResult<UserListItemDto>>> Handle(
    GetUsersQuery query,
    CancellationToken cancellationToken)
  {
    var usersQuery = context.Users.AsNoTracking().AsQueryable();

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

    if (query.Statuses?.Count > 0)
      usersQuery = usersQuery.Where(u => query.Statuses.Contains(u.Status));

    var projectedUsers = usersQuery.Select(u => new UserListItemDto
    {
      Id = u.Id,
      FullName = u.FirstName + " " + u.LastName,
      Email = u.Email,
      Status = u.Status,
      RoleNames = u.Roles
        .Select(ur => ur.Role.Name)
        .OrderBy(name => name)
        .ToList(),
      LastSignInAt = context.UserSessions
        .Where(s => s.UserId == u.Id)
        .OrderByDescending(s => s.SignedInAt)
        .Select(s => (DateTime?)s.SignedInAt)
        .FirstOrDefault(),
      CreatedAt = u.CreatedAt
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedUsers = await projectedUsers.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedUsers);
  }

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "Name" or "FullName" => nameof(UserListItemDto.FullName),
      "CreatedAt" => nameof(UserListItemDto.CreatedAt),
      "LastSignIn" or "LastSignInAt" => nameof(UserListItemDto.LastSignInAt),
      _ => nameof(UserListItemDto.FullName)
    };
}

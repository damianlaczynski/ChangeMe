using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public class GetUsersQuery : PaginationQuery<UserListItemDto>
{
  public string? SearchText { get; set; }
  public List<bool>? Deactivated { get; set; }
  public List<bool>? EmailVerified { get; set; }
  public List<UserMembershipStatus>? Status { get; set; }
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

    if (query.Deactivated?.Count > 0)
      usersQuery = usersQuery.Where(u => query.Deactivated.Contains(u.Deactivated));

    if (query.EmailVerified?.Count > 0)
      usersQuery = usersQuery.Where(u => query.EmailVerified.Contains(u.EmailVerified));

    if (query.Status?.Count > 0)
      usersQuery = ApplyStatusFilter(usersQuery, query.Status);

    var projectedUsers = usersQuery
      .WithMembershipFlags()
      .Select(x => new
      {
        x.User.Id,
        x.User.FirstName,
        x.User.LastName,
        x.User.Email,
        x.User.Deactivated,
        x.User.HasPasswordSet,
        x.User.EmailVerified,
        x.InvitationPending,
        x.HasExternalLogin,
        RoleNames = x.User.Roles
          .Select(ur => ur.Role.Name)
          .OrderBy(name => name)
          .ToList(),
        LastSignInAt = context.UserSessions
          .Where(s => s.UserId == x.User.Id)
          .OrderByDescending(s => s.SignedInAt)
          .Select(s => (DateTime?)s.SignedInAt)
          .FirstOrDefault(),
        x.User.CreatedAt
      })
      .Select(x => new UserListItemDto
      {
        Id = x.Id,
        FirstName = x.FirstName,
        LastName = x.LastName,
        Email = x.Email,
        Deactivated = x.Deactivated,
        HasPasswordSet = x.HasPasswordSet,
        EmailVerified = x.EmailVerified,
        InvitationPending = x.InvitationPending,
        HasExternalLogin = x.HasExternalLogin,
        Status = UsersStatusUtils.ComputeStatus(
          x.Deactivated,
          x.InvitationPending,
          x.HasPasswordSet,
          x.HasExternalLogin),
        RoleNames = x.RoleNames,
        LastSignInAt = x.LastSignInAt,
        CreatedAt = x.CreatedAt
      });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedUsers = await projectedUsers.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedUsers);
  }

  private static IQueryable<User> ApplyStatusFilter(
    IQueryable<User> usersQuery,
    List<UserMembershipStatus> statuses) =>
    usersQuery
      .WithMembershipFlags()
      .Where(x =>
        (statuses.Contains(UserMembershipStatus.Deactivated) && x.User.Deactivated)
        || (statuses.Contains(UserMembershipStatus.Invited) && !x.User.Deactivated && x.InvitationPending)
        || (statuses.Contains(UserMembershipStatus.InvitationCanceled)
            && !x.User.Deactivated
            && !x.InvitationPending
            && !x.User.HasPasswordSet
            && !x.HasExternalLogin)
        || (statuses.Contains(UserMembershipStatus.Active)
            && !x.User.Deactivated
            && !x.InvitationPending
            && (x.User.HasPasswordSet || x.HasExternalLogin)))
      .Select(x => x.User);

  private static string MapSortField(string sortField) =>
    sortField switch
    {
      "Name" or "DisplayName" or "FullName" or "LastName" => nameof(UserListItemDto.LastName),
      "FirstName" => nameof(UserListItemDto.FirstName),
      "Email" => nameof(UserListItemDto.Email),
      "CreatedAt" => nameof(UserListItemDto.CreatedAt),
      "LastSignIn" or "LastSignInAt" => nameof(UserListItemDto.LastSignInAt),
      _ => nameof(UserListItemDto.LastName)
    };
}

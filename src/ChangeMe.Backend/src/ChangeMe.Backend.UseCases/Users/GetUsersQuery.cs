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
  public async ValueTask<Result<PaginationResult<UserListItemDto>>> Handle(
    GetUsersQuery query,
    CancellationToken cancellationToken)
  {
    var usersQuery = context.Users.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
      usersQuery = usersQuery.Where(u =>
        EF.Functions.ILike(u.FirstName, $"%{searchText}%")
        || EF.Functions.ILike(u.LastName, $"%{searchText}%")
        || EF.Functions.ILike(u.Email, $"%{searchText}%"));
    }

    if (query.Deactivated?.Count > 0)
      usersQuery = usersQuery.Where(u => query.Deactivated.Contains(u.Deactivated));

    if (query.EmailVerified?.Count > 0)
      usersQuery = usersQuery.Where(u => query.EmailVerified.Contains(u.EmailVerified));

    var flaggedUsers = usersQuery.WithMembershipFlags();

    if (query.Status?.Count > 0)
      flaggedUsers = ApplyStatusFilter(flaggedUsers, query.Status);

    var projectedUsers = flaggedUsers.Select(x => new UserListItemDto
    {
      Id = x.User.Id,
      FirstName = x.User.FirstName,
      LastName = x.User.LastName,
      Email = x.User.Email,
      Deactivated = x.User.Deactivated,
      HasPasswordSet = x.User.HasPasswordSet,
      EmailVerified = x.User.EmailVerified,
      InvitationPending = x.InvitationPending,
      HasExternalLogin = x.HasExternalLogin,
      Status = UsersStatusUtils.ComputeStatus(
        x.User.Deactivated,
        x.InvitationPending,
        x.User.HasPasswordSet,
        x.HasExternalLogin),
      RoleNames = x.User.Roles
        .Select(ur => ur.Role.Name)
        .OrderBy(name => name)
        .ToList(),
      LastSignInAt = context.UserSessions
        .Where(s => s.UserId == x.User.Id)
        .OrderByDescending(s => s.SignedInAt)
        .Select(s => (DateTime?)s.SignedInAt)
        .FirstOrDefault(),
      CreatedAt = x.User.CreatedAt
    });

    query.PaginationParameters.SortField = MapSortField(query.PaginationParameters.SortField);

    var pagedUsers = await projectedUsers.ToPaginationResultAsync(
      x => x,
      query.PaginationParameters,
      cancellationToken);

    return Result.Success(pagedUsers);
  }

  private static IQueryable<UserMembershipFlags> ApplyStatusFilter(
    IQueryable<UserMembershipFlags> flaggedUsers,
    List<UserMembershipStatus> statuses) =>
    flaggedUsers.Where(x =>
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
          && (x.User.HasPasswordSet || x.HasExternalLogin)));

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

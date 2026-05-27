using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.UseCases.Users.Utils;

internal static class UserQueryExpressions
{
  internal static IQueryable<UserMembershipFlags> WithMembershipFlags(this IQueryable<User> users) =>
    users.Select(u => new UserMembershipFlags
    {
      User = u,
      InvitationPending = u.AccountInvitations.Any(i => i.AcceptedAtUtc == null && i.RevokedAtUtc == null),
      HasExternalLogin = u.ExternalLogins.Any(),
    });
}

internal sealed class UserMembershipFlags
{
  public required User User { get; init; }
  public bool InvitationPending { get; init; }
  public bool HasExternalLogin { get; init; }
}

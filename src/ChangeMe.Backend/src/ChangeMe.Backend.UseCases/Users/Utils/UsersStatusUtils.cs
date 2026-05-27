using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users.Utils;

public static class UsersStatusUtils
{
  public static UserMembershipStatus ComputeStatus(
    bool deactivated,
    bool invitationPending,
    bool hasPasswordSet,
    bool hasExternalLogin) =>
    deactivated
      ? UserMembershipStatus.Deactivated
      : invitationPending
        ? UserMembershipStatus.Invited
        : !hasPasswordSet && !hasExternalLogin
          ? UserMembershipStatus.InvitationCanceled
          : UserMembershipStatus.Active;

  public static UserMembershipStatus ComputeStatus(User user) =>
    ComputeStatus(
      user.Deactivated,
      user.HasPendingInvitation,
      user.HasPasswordSet,
      user.ExternalLogins.Count > 0);
}

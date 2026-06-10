using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users.Utils;

public static class UsersMappingExtensions
{
  public static UserDetailsDto ToDetailsDto(
    this User user,
    DateTime? lastSignInAt,
    IReadOnlyList<UserRoleSummaryDto> roles,
    IReadOnlyList<EffectivePermissionDto> effectivePermissions,
    IReadOnlyList<UserExternalLoginDto> externalLogins,
    UserInvitationInfoDto? pendingInvitation,
    IReadOnlyList<UserPasskeyDto> passkeys,
    IPasswordExpirationEvaluator? passwordExpirationEvaluator = null) =>
    new()
    {
      Id = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      Deactivated = user.Deactivated,
      DeactivatedAt = user.DeactivatedAt,
      HasPasswordSet = user.HasPasswordSet,
      EmailVerified = user.EmailVerified,
      EmailVerifiedAt = user.EmailVerifiedAt,
      PasswordLastChangedAt = user.PasswordLastChangedAt,
      PasswordExpiresAtUtc = passwordExpirationEvaluator?.GetPasswordExpiresAtUtc(user),
      TwoFactorEnabled = user.TwoFactorEnabled,
      TwoFactorEnabledAt = user.TwoFactorEnabledAt,
      InvitationPending = user.HasPendingInvitation,
      Status = UsersStatusUtils.ComputeStatus(user),
      PendingInvitation = pendingInvitation,
      PendingEmailChange = user.HasPendingEmailChange
        && user.PendingNewEmail is not null
        && user.PendingEmailChangeRequestedAtUtc is not null
        ? new PendingEmailChangeDto(user.PendingNewEmail, user.PendingEmailChangeRequestedAtUtc.Value)
        : null,
      MemberSince = user.CreatedAt,
      LastSignInAt = lastSignInAt,
      Roles = roles,
      EffectivePermissions = effectivePermissions,
      ExternalLogins = externalLogins,
      Passkeys = passkeys
    };
}

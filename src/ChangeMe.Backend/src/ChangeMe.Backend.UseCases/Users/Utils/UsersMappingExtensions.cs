using ChangeMe.Backend.Domain.Aggregates.Users;
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
    int invitationLinkLifetimeHours,
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
      PendingInvitation = user.GetPendingInvitationSummary() is { } summary
        ? new UserInvitationInfoDto(
          summary.LastSentAtUtc,
          summary.SentCount,
          summary.LastSentAtUtc.AddHours(invitationLinkLifetimeHours))
        : null,
      MemberSince = user.CreatedAt,
      LastSignInAt = lastSignInAt,
      Roles = roles,
      EffectivePermissions = effectivePermissions,
      ExternalLogins = externalLogins
    };
}

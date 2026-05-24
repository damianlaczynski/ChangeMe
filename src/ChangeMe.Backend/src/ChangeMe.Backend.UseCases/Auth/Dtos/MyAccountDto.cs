using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record MyAccountDto(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  DateTime MemberSince,
  bool HasPasswordSet,
  bool TwoFactorEnabled,
  DateTime? TwoFactorEnabledAt,
  bool ExternalStepUpFresh,
  IReadOnlyList<UserRoleSummaryDto> Roles,
  IReadOnlyList<EffectivePermissionDto> EffectivePermissions,
  IReadOnlyList<MyAccountExternalLoginDto> ExternalLogins,
  IReadOnlyList<ExternalProviderSettingsDto> LinkableProviders,
  IReadOnlyList<MyAccountPasskeyDto> Passkeys,
  bool PasskeyStepUpFresh);

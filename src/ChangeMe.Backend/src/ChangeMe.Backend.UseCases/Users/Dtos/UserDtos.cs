namespace ChangeMe.Backend.UseCases.Users.Dtos;

public sealed record UserListItemDto
{
  public Guid Id { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public bool Deactivated { get; init; }
  public bool HasPasswordSet { get; init; }
  public bool EmailVerified { get; init; }
  public bool InvitationPending { get; init; }
  public bool HasExternalLogin { get; init; }
  public UserMembershipStatus Status { get; init; }
  public IReadOnlyList<string> RoleNames { get; init; } = [];
  public DateTime? LastSignInAt { get; init; }
  public DateTime CreatedAt { get; init; }
}

public sealed record UserRoleSummaryDto(Guid Id, string Name, bool IsSystem);

public sealed record EffectivePermissionDto(
  string Code,
  string Label,
  string Description,
  string Group,
  IReadOnlyList<string> FromRoleNames);

public sealed record UserDetailsDto
{
  public Guid Id { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public bool Deactivated { get; init; }
  public DateTime? DeactivatedAt { get; init; }
  public bool HasPasswordSet { get; init; }
  public bool EmailVerified { get; init; }
  public DateTime? EmailVerifiedAt { get; init; }
  public DateTime? PasswordLastChangedAt { get; init; }
  public DateTime? PasswordExpiresAtUtc { get; init; }
  public bool TwoFactorEnabled { get; init; }
  public DateTime? TwoFactorEnabledAt { get; init; }
  public bool InvitationPending { get; init; }
  public UserMembershipStatus Status { get; init; }
  public UserInvitationInfoDto? PendingInvitation { get; init; }
  public DateTime MemberSince { get; init; }
  public DateTime? LastSignInAt { get; init; }
  public IReadOnlyList<UserRoleSummaryDto> Roles { get; init; } = [];
  public IReadOnlyList<EffectivePermissionDto> EffectivePermissions { get; init; } = [];
  public IReadOnlyList<UserExternalLoginDto> ExternalLogins { get; init; } = [];
  public IReadOnlyList<UserPasskeyDto> Passkeys { get; init; } = [];
}

public sealed record UserPasskeyDto(
  Guid Id,
  string Name,
  DateTime CreatedAtUtc,
  DateTime? LastUsedAtUtc,
  string AuthenticatorType,
  bool BackupEligible,
  bool BackupState);

public sealed record UserExternalLoginDto(
  string ProviderKey,
  string DisplayName,
  DateTime LinkedAtUtc);

public sealed record RoleAssignmentOptionDto(Guid Id, string Name, bool IsSystem);

public sealed record AdminUserSessionDto(
  Guid Id,
  string DeviceBrowserLabel,
  string SignInMethod,
  string SignInMethodLabel,
  string? IpAddress,
  DateTime SignedInAt,
  DateTime LastActivityAt);

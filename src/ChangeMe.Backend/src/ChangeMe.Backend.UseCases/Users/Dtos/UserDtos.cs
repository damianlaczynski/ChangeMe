using ChangeMe.Backend.Domain.Authorization;

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
  public DateTime? InvitationSentAt { get; init; }
  public DateTime MemberSince { get; init; }
  public DateTime? LastSignInAt { get; init; }
  public IReadOnlyList<UserRoleSummaryDto> Roles { get; init; } = [];
  public IReadOnlyList<EffectivePermissionDto> EffectivePermissions { get; init; } = [];
}

public sealed record RoleAssignmentOptionDto(Guid Id, string Name, bool IsSystem);

public sealed record AdminUserSessionDto(
  Guid Id,
  string DeviceBrowserLabel,
  string? IpAddress,
  bool IsPersistent,
  DateTime SignedInAt,
  DateTime LastActivityAt);

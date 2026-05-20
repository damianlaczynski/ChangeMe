using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UseCases.Users.Dtos;

public sealed record UserListItemDto
{
  public Guid Id { get; init; }
  public string FullName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public UserStatus Status { get; init; }
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
  public string FullName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public UserStatus Status { get; init; }
  public DateTime MemberSince { get; init; }
  public DateTime? LastSignInAt { get; init; }
  public IReadOnlyList<UserRoleSummaryDto> Roles { get; init; } = [];
  public IReadOnlyList<EffectivePermissionDto> EffectivePermissions { get; init; } = [];
}

public sealed record UserFormDto
{
  public Guid Id { get; init; }
  public string FirstName { get; init; } = string.Empty;
  public string LastName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public UserStatus Status { get; init; }
  public IReadOnlyList<Guid> RoleIds { get; init; } = [];
}

public sealed record RoleAssignmentOptionDto(Guid Id, string Name, bool IsSystem);

public sealed record AdminUserSessionDto(
  Guid Id,
  string DeviceBrowserLabel,
  string? IpAddress,
  bool IsPersistent,
  DateTime SignedInAt,
  DateTime LastActivityAt);

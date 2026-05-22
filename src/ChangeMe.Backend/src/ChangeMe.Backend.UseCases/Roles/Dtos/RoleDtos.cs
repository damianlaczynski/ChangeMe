using ChangeMe.Backend.Domain.Aggregates.Users;

namespace ChangeMe.Backend.UseCases.Roles.Dtos;

public sealed record RoleListItemDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? Description { get; init; }
  public int PermissionCount { get; init; }
  public int UserCount { get; init; }
  public bool IsSystem { get; init; }
}

public sealed record PermissionCatalogItemDto(
  string Code,
  string Label,
  string Description,
  string Group);

public sealed record RolePermissionItemDto(
  string Code,
  string Label,
  string Description,
  string Group);

public sealed record RoleDetailsDto
{
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? Description { get; init; }
  public bool IsSystem { get; init; }
  public int PermissionCount { get; init; }
  public int UserCount { get; init; }
  public IReadOnlyList<RolePermissionItemDto> Permissions { get; init; } = [];
}

public sealed record RoleAssignedUserDto
{
  public Guid Id { get; init; }
  public string FullName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public UserStatus Status { get; init; }
}


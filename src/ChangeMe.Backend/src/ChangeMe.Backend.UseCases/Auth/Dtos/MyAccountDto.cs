using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Auth.Dtos;

public sealed record MyAccountDto(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  string Status,
  DateTime MemberSince,
  IReadOnlyList<UserRoleSummaryDto> Roles,
  IReadOnlyList<EffectivePermissionDto> EffectivePermissions);

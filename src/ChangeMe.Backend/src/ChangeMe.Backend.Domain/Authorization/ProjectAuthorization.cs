using ChangeMe.Backend.Domain.Aggregates.Project.Enums;

namespace ChangeMe.Backend.Domain.Authorization;

public static class ProjectAuthorization
{
  public static readonly Guid SystemActorUserId = Guid.Empty;

  public static bool HasPermission(ProjectRole? role, string permissionCode) =>
    role.HasValue && ProjectPermissionCodes.RoleHasPermission(role.Value, permissionCode);
}

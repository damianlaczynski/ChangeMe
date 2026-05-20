using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Users;

public static class UsersSupport
{
  public const string DuplicateEmailMessage = "A user with this email already exists.";
  public const string CannotRemoveOwnAdministratorMessage = "You cannot remove your own administrator access.";
  public const string CannotDeactivateOwnAccountMessage = "You cannot deactivate your own account.";
  public const string CannotChangeOwnRolesMessage = "You cannot change your own roles.";
  public const string AtLeastOneRoleRequiredMessage = "At least one role is required.";
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  public static async Task<DateTime?> GetLastSignInAtAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    return await context.UserSessions
      .AsNoTracking()
      .Where(x => x.UserId == userId)
      .OrderByDescending(x => x.SignedInAt)
      .Select(x => (DateTime?)x.SignedInAt)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public static async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsForUserAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var grants = await context.Users
      .AsNoTracking()
      .Where(x => x.Id == userId)
      .SelectMany(x => x.Roles)
      .SelectMany(x => x.Role.Permissions.Select(p => new { p.PermissionCode, x.Role.Name }))
      .ToListAsync(cancellationToken);

    return PermissionCatalog
      .BuildEffectivePermissions(grants.Select(x => (x.PermissionCode, x.Name)))
      .Select(x => new EffectivePermissionDto(
        x.Code,
        x.Label,
        x.Description,
        x.Group,
        x.FromRoleNames))
      .ToList();
  }

  public static async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsForRolesAsync(
    ApplicationDbContext context,
    IReadOnlyList<Guid> roleIds,
    CancellationToken cancellationToken)
  {
    if (roleIds.Count == 0)
      return [];

    var grants = await context.Roles
      .AsNoTracking()
      .Where(x => roleIds.Contains(x.Id))
      .SelectMany(x => x.Permissions.Select(p => new { p.PermissionCode, x.Name }))
      .ToListAsync(cancellationToken);

    return PermissionCatalog
      .BuildEffectivePermissions(grants.Select(x => (x.PermissionCode, x.Name)))
      .Select(x => new EffectivePermissionDto(
        x.Code,
        x.Label,
        x.Description,
        x.Group,
        x.FromRoleNames))
      .ToList();
  }

  public static async Task<Result> ReplaceUserRolesAsync(
    ApplicationDbContext context,
    Guid userId,
    IReadOnlyList<Guid> roleIds,
    Guid actingUserId,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .Include(x => x.Roles)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    if (user is null)
      return Result.NotFound();

    var distinctRoleIds = roleIds.Distinct().ToList();
    var existingRoleCount = await context.Roles
      .CountAsync(x => distinctRoleIds.Contains(x.Id), cancellationToken);

    if (existingRoleCount != distinctRoleIds.Count)
      return Result.NotFound();

    if (actingUserId == userId)
    {
      var administratorRoleId = await context.Roles
        .AsNoTracking()
        .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
        .Select(x => x.Id)
        .FirstOrDefaultAsync(cancellationToken);

      if (administratorRoleId != Guid.Empty
          && user.HasRole(administratorRoleId)
          && !distinctRoleIds.Contains(administratorRoleId))
        return Result.Error(CannotRemoveOwnAdministratorMessage);
    }

    return user.ReplaceRoles(distinctRoleIds);
  }

  public static async Task RevokeAllActiveSessionsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var sessions = await context.UserSessions
      .Where(x => x.UserId == userId && x.RevokedAt == null)
      .ToListAsync(cancellationToken);

    foreach (var session in sessions)
      session.Revoke(utcNow);
  }

  public static IReadOnlyList<AdminUserSessionDto> MapActiveSessions(
    IEnumerable<UserSession> sessions,
    DateTime utcNow,
    ISessionLifetimeService sessionLifetime)
  {
    return sessions
      .Where(x => sessionLifetime.IsActive(x, utcNow))
      .OrderByDescending(x => x.LastActivityAt)
      .Select(x => new AdminUserSessionDto(
        x.Id,
        x.DeviceBrowserLabel,
        x.IpAddress,
        x.IsPersistent,
        x.SignedInAt,
        x.LastActivityAt))
      .ToList();
  }
}

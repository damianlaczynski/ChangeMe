namespace ChangeMe.Backend.Domain.Authorization;

public sealed record PermissionDefinition(
  string Code,
  string Label,
  string Description,
  string Group);

public sealed record EffectivePermissionEntry(
  string Code,
  string Label,
  string Description,
  string Group,
  IReadOnlyList<string> FromRoleNames);

public static class PermissionCatalog
{
  public static readonly IReadOnlyList<PermissionDefinition> All =
  [
    new(PermissionCodes.UsersView, "View users", "View the users list, user details, and read-only role badges on user screens.", "Users"),
    new(PermissionCodes.UsersManage, "Manage users", "Create and edit user profile data (name, email).", "Users"),
    new(PermissionCodes.UsersDeactivate, "Deactivate users", "Deactivate and reactivate user accounts.", "Users"),
    new(PermissionCodes.RolesView, "View roles", "View the roles list and role details.", "Roles"),
    new(PermissionCodes.RolesManage, "Manage roles", "Create, edit, and delete custom roles; manage role and user assignments.", "Roles"),
    new(PermissionCodes.SessionsViewOwn, "View own sessions", "View the current user's active sessions on My sessions.", "Sessions"),
    new(PermissionCodes.SessionsManageOwn, "Manage own sessions", "Revoke non-current own sessions and use Sign out everywhere.", "Sessions"),
    new(PermissionCodes.SessionsViewAny, "View user sessions", "View active sessions of any user in User details.", "Sessions"),
    new(PermissionCodes.SessionsManageAny, "Manage user sessions", "Revoke sessions of any user, including Revoke all sessions.", "Sessions")
  ];

  private static readonly Dictionary<string, PermissionDefinition> DefinitionByCode =
    All.ToDictionary(x => x.Code, StringComparer.Ordinal);

  public static PermissionDefinition? Find(string code) =>
    DefinitionByCode.TryGetValue(code, out var definition) ? definition : null;

  public static IReadOnlyList<EffectivePermissionEntry> BuildEffectivePermissions(
    IEnumerable<(string PermissionCode, string RoleName)> grants)
  {
    return grants
      .GroupBy(x => x.PermissionCode, StringComparer.Ordinal)
      .Select(group =>
      {
        var definition = Find(group.Key);
        var roleNames = group.Select(x => x.RoleName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
        return new EffectivePermissionEntry(
          group.Key,
          definition?.Label ?? group.Key,
          definition?.Description ?? string.Empty,
          definition?.Group ?? "Other",
          roleNames);
      })
      .OrderBy(x => x.Group, StringComparer.Ordinal)
      .ThenBy(x => x.Label, StringComparer.Ordinal)
      .ToList();
  }
}

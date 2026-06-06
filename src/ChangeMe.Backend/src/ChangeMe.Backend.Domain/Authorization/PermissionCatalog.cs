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
    new(PermissionCodes.SessionsViewOwn, "View own sessions", "View the current user's active sessions on My account.", "Sessions"),
    new(PermissionCodes.SessionsManageOwn, "Manage own sessions", "Revoke non-current own sessions and use Sign out everywhere.", "Sessions"),
    new(PermissionCodes.SessionsViewAny, "View user sessions", "View active sessions of any user in User details.", "Sessions"),
    new(PermissionCodes.SessionsManageAny, "Manage user sessions", "Revoke sessions of any user, including Revoke all sessions.", "Sessions"),
    new(PermissionCodes.TimeViewOwn, "View own time entries", "Open My time and view own time entries.", "Time"),
    new(PermissionCodes.TimeLogOwn, "Log own time", "Create time entries and use the running timer (subject to project membership and project permissions).", "Time"),
    new(PermissionCodes.TimeManageOwn, "Manage own time entries", "Edit and delete own time entries (subject to backdating rules unless Time.LogPastLimit applies).", "Time"),
    new(PermissionCodes.TimeViewAny, "View all time entries", "View any user's time entries on My time (when viewing as another user) and in cross-user contexts that reference this permission.", "Time"),
    new(PermissionCodes.TimeManageAny, "Manage all time entries", "Edit and delete any user's time entries globally.", "Time"),
    new(PermissionCodes.TimeViewReports, "View time reports", "Open Time reports, run grouped reports, export CSV, and read the time entry operation audit log.", "Time"),
    new(PermissionCodes.TimeLogPastLimit, "Log time outside backdating limit", "Create and edit time entries with Work date older than Time backdating limit (days).", "Time"),
    new(PermissionCodes.BillingViewOwn, "View own billing data", "Open My leave, My availability, and My billing; view own leave balance, requests, availability, and published settlements.", "Billing"),
    new(PermissionCodes.BillingViewAny, "View all billing data", "View employment profiles, contracts, leave, and team availability for any user in admin contexts.", "Billing"),
    new(PermissionCodes.BillingManageEmployment, "Manage employment data", "Create and edit positions, employment profiles, and contracts.", "Billing"),
    new(PermissionCodes.BillingManageLeave, "Manage leave requests", "Create, edit, and cancel leave requests for any user.", "Billing"),
    new(PermissionCodes.BillingApproveLeave, "Approve leave requests", "Approve or reject Submitted leave requests.", "Billing"),
    new(PermissionCodes.BillingViewReports, "View billing reports", "Open Billing reports, run grouped analysis, export CSV, and read settlement operation history.", "Billing"),
    new(PermissionCodes.BillingManageSettlements, "Manage settlements", "Create settlement periods, recalculate user settlements, and close periods.", "Billing"),
    new(PermissionCodes.BillingManageOwnAvailability, "Manage own availability", "Create, edit, and delete own Manual availability entries and weekly recurring pattern.", "Billing"),
    new(PermissionCodes.BillingManageAvailability, "Manage user availability", "Create, edit, and delete availability entries and weekly recurring pattern for any user.", "Billing")
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

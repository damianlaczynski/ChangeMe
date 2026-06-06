using ChangeMe.Backend.Domain.Aggregates.Project.Enums;

namespace ChangeMe.Backend.Domain.Authorization;

public static class ProjectPermissionCodes
{
  public const string View = "Project.View";
  public const string Manage = "Project.Manage";
  public const string MembersView = "Project.Members.View";
  public const string MembersManage = "Project.Members.Manage";
  public const string IssuesView = "Project.Issues.View";
  public const string IssuesManage = "Project.Issues.Manage";
  public const string TimeLog = "Project.Time.Log";
  public const string TimeView = "Project.Time.View";
  public const string TimeManage = "Project.Time.Manage";

  public const string ForbiddenMessage = "You do not have permission to perform this action on this project.";

  public static readonly IReadOnlyList<string> All =
  [
    View,
    Manage,
    MembersView,
    MembersManage,
    IssuesView,
    IssuesManage,
    TimeLog,
    TimeView,
    TimeManage,
  ];

  private static readonly IReadOnlyDictionary<ProjectRole, IReadOnlyList<string>> RolePermissions =
    new Dictionary<ProjectRole, IReadOnlyList<string>>
    {
      [ProjectRole.OWNER] = All,
      [ProjectRole.MEMBER] =
      [
        View,
        MembersView,
        IssuesView,
        IssuesManage,
        TimeLog,
        TimeView,
      ],
      [ProjectRole.VIEWER] =
      [
        View,
        MembersView,
        IssuesView,
        TimeView,
      ],
    };

  public static bool RoleHasPermission(ProjectRole role, string permissionCode) =>
    RolePermissions[role].Contains(permissionCode);

  public static IReadOnlyList<string> GetPermissionsForRole(ProjectRole role) =>
    RolePermissions[role];
}

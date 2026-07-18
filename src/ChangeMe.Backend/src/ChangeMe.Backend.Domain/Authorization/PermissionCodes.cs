namespace ChangeMe.Backend.Domain.Authorization;

public static class PermissionCodes
{
  public const string UsersView = "Users.View";
  public const string UsersManage = "Users.Manage";
  public const string UsersDeactivate = "Users.Deactivate";
  public const string UsersInvite = "Users.Invite";
  public const string RolesView = "Roles.View";
  public const string RolesManage = "Roles.Manage";
  public const string SessionsViewOwn = "Sessions.ViewOwn";
  public const string SessionsManageOwn = "Sessions.ManageOwn";
  public const string SessionsViewAny = "Sessions.ViewAny";
  public const string SessionsManageAny = "Sessions.ManageAny";
  public const string IssuesView = "Issues.View";
  public const string IssuesCreate = "Issues.Create";
  public const string IssuesEdit = "Issues.Edit";
  public const string IssuesDelete = "Issues.Delete";
  public const string IssuesComment = "Issues.Comment";
  public const string IssuesManageAttachments = "Issues.ManageAttachments";

  public static readonly IReadOnlyList<string> All =
  [
    UsersView,
    UsersManage,
    UsersDeactivate,
    UsersInvite,
    RolesView,
    RolesManage,
    SessionsViewOwn,
    SessionsManageOwn,
    SessionsViewAny,
    SessionsManageAny,
    IssuesView,
    IssuesCreate,
    IssuesEdit,
    IssuesDelete,
    IssuesComment,
    IssuesManageAttachments
  ];

  public static readonly IReadOnlyList<string> DefaultUserRole =
  [
    SessionsViewOwn,
    SessionsManageOwn,
    IssuesView,
    IssuesCreate,
    IssuesComment
  ];
}

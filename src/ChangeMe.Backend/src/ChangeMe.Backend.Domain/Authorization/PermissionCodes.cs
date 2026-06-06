namespace ChangeMe.Backend.Domain.Authorization;

public static class PermissionCodes
{
  public const string UsersView = "Users.View";
  public const string UsersManage = "Users.Manage";
  public const string UsersDeactivate = "Users.Deactivate";
  public const string RolesView = "Roles.View";
  public const string RolesManage = "Roles.Manage";
  public const string SessionsViewOwn = "Sessions.ViewOwn";
  public const string SessionsManageOwn = "Sessions.ManageOwn";
  public const string SessionsViewAny = "Sessions.ViewAny";
  public const string SessionsManageAny = "Sessions.ManageAny";
  public const string TimeViewOwn = "Time.ViewOwn";
  public const string TimeLogOwn = "Time.LogOwn";
  public const string TimeManageOwn = "Time.ManageOwn";
  public const string TimeViewAny = "Time.ViewAny";
  public const string TimeManageAny = "Time.ManageAny";
  public const string TimeViewReports = "Time.ViewReports";
  public const string TimeLogPastLimit = "Time.LogPastLimit";
  public const string BillingViewOwn = "Billing.ViewOwn";
  public const string BillingViewAny = "Billing.ViewAny";
  public const string BillingManageEmployment = "Billing.ManageEmployment";
  public const string BillingManageLeave = "Billing.ManageLeave";
  public const string BillingApproveLeave = "Billing.ApproveLeave";
  public const string BillingViewReports = "Billing.ViewReports";
  public const string BillingManageSettlements = "Billing.ManageSettlements";
  public const string BillingManageOwnAvailability = "Billing.ManageOwnAvailability";
  public const string BillingManageAvailability = "Billing.ManageAvailability";

  public static readonly IReadOnlyList<string> All =
  [
    UsersView,
    UsersManage,
    UsersDeactivate,
    RolesView,
    RolesManage,
    SessionsViewOwn,
    SessionsManageOwn,
    SessionsViewAny,
    SessionsManageAny,
    TimeViewOwn,
    TimeLogOwn,
    TimeManageOwn,
    TimeViewAny,
    TimeManageAny,
    TimeViewReports,
    TimeLogPastLimit,
    BillingViewOwn,
    BillingViewAny,
    BillingManageEmployment,
    BillingManageLeave,
    BillingApproveLeave,
    BillingViewReports,
    BillingManageSettlements,
    BillingManageOwnAvailability,
    BillingManageAvailability,
  ];

  public static readonly IReadOnlyList<string> DefaultUserRole =
  [
    SessionsViewOwn,
    SessionsManageOwn,
    TimeViewOwn,
    TimeLogOwn,
    TimeManageOwn,
    BillingViewOwn,
    BillingManageOwnAvailability,
  ];
}

using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class BillingUtils
{
  public static Result RequireAnyPermission(IUserAccessor userAccessor, params string[] permissionCodes)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    if (permissionCodes.Any(userAccessor.HasPermission))
      return Result.Success();

    return Result.Forbidden();
  }

  public static Result RequirePermission(IUserAccessor userAccessor, string permissionCode)
  {
    if (userAccessor.UserId is null)
      return Result.Unauthorized();

    return userAccessor.HasPermission(permissionCode)
      ? Result.Success()
      : Result.Forbidden();
  }

  public static bool CanManageEmployment(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingManageEmployment);

  public static bool CanViewBillingData(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingViewAny)
    || userAccessor.HasPermission(PermissionCodes.BillingManageEmployment);

  public static bool CanViewBillingReports(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingViewReports);

  public static bool CanManageSettlements(IUserAccessor userAccessor) =>
    userAccessor.HasPermission(PermissionCodes.BillingManageSettlements);
}

using ChangeMe.Backend.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ChangeMe.Backend.Web.Authorization;

public static class BillingAuthorizationPolicies
{
  public const string ViewEmployment = "Billing.Policy.ViewEmployment";

  public static void RegisterPolicies(AuthorizationOptions options)
  {
    options.AddPolicy(ViewEmployment, policy =>
      policy.RequireAssertion(context =>
        HasPermission(context.User, PermissionCodes.BillingViewAny)
        || HasPermission(context.User, PermissionCodes.BillingManageEmployment)));
  }

  private static bool HasPermission(System.Security.Claims.ClaimsPrincipal user, string permissionCode) =>
    user.HasClaim(PermissionClaimTypes.Permission, permissionCode);
}

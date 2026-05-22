using ChangeMe.Backend.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ChangeMe.Backend.Web.Authorization;

public static class PermissionAuthorizationExtensions
{
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
  {
    services.AddAuthorization(options =>
    {
      foreach (var permission in PermissionCodes.All)
      {
        options.AddPolicy(permission, policy =>
          policy.RequireClaim(PermissionClaimTypes.Permission, permission));
      }
    });

    services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionDeniedAuthorizationResultHandler>();

    return services;
  }
}

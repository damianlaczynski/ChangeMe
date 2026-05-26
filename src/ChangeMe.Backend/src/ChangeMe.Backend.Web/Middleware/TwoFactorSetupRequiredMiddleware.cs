using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.Web.Middleware;

public sealed class TwoFactorSetupRequiredMiddleware(RequestDelegate next)
{
  private static readonly string[] AllowedPathPrefixes =
  [
    "/api/auth/account",
    "/api/auth/required-change-password",
    "/api/auth/two-factor/setup/begin",
    "/api/auth/two-factor/setup/confirm",
    "/api/auth/external/",
    "/api/auth/logout",
    "/api/auth/refresh"
  ];

  public async Task InvokeAsync(
    HttpContext context,
    ApplicationDbContext db,
    IUserAccessor userAccessor,
    IPasswordExpirationEvaluator passwordExpirationEvaluator,
    ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator)
  {
    if (context.User.Identity?.IsAuthenticated != true || IsAllowedPath(context.Request.Path))
    {
      await next(context);
      return;
    }

    if (userAccessor.UserId is not Guid userId)
    {
      await next(context);
      return;
    }

    var user = await db.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.Id == userId, context.RequestAborted);
    if (user is null)
    {
      await next(context);
      return;
    }

    var utcNow = DateTime.UtcNow;
    if (passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow))
    {
      await next(context);
      return;
    }

    if (!twoFactorPolicyEvaluator.IsTwoFactorSetupRequired(user))
    {
      await next(context);
      return;
    }

    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(
      Result.Forbidden("Two-factor authentication setup is required to continue."),
      context.RequestAborted);
  }

  private static bool IsAllowedPath(PathString path)
  {
    var value = path.Value;
    if (string.IsNullOrEmpty(value))
      return false;

    return AllowedPathPrefixes.Any(prefix =>
      value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
  }
}

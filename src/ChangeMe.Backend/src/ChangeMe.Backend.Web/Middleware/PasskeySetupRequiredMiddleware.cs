using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.Web.Middleware;

public sealed class PasskeySetupRequiredMiddleware(RequestDelegate next)
{
  private static readonly string[] AllowedPathPrefixes =
  [
    "/api/auth/passkeys/register/begin",
    "/api/auth/passkeys/register/complete",
    "/api/auth/passkeys/step-up/begin",
    "/api/auth/passkeys/step-up/complete",
    "/api/auth/logout",
    "/api/auth/refresh"
  ];

  public async Task InvokeAsync(
    HttpContext context,
    ApplicationDbContext db,
    IUserAccessor userAccessor,
    IPasswordExpirationEvaluator passwordExpirationEvaluator,
    ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
    IPasskeyPolicyEvaluator passkeyPolicyEvaluator)
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

    if (!passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment())
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
    if (passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow)
        || twoFactorPolicyEvaluator.IsTwoFactorSetupRequired(user))
    {
      await next(context);
      return;
    }

    var passkeyCount = await db.PasskeyCredentials.CountAsync(x => x.UserId == userId, context.RequestAborted);
    if (!passkeyPolicyEvaluator.IsPasskeySetupRequired(user, passkeyCount))
    {
      await next(context);
      return;
    }

    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(
      Result.Forbidden("Passkey setup is required to continue."),
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

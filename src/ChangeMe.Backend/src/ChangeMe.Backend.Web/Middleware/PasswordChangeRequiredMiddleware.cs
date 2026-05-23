using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.Web.Middleware;

public sealed class PasswordChangeRequiredMiddleware(RequestDelegate next)
{
  private static readonly string[] AllowedPathPrefixes =
  [
    "/api/auth/required-change-password",
    "/api/auth/logout",
    "/api/auth/refresh"
  ];

  public async Task InvokeAsync(
    HttpContext context,
    ApplicationDbContext db,
    IUserAccessor userAccessor,
    IPasswordExpirationEvaluator passwordExpirationEvaluator)
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
    if (user is null || !passwordExpirationEvaluator.IsPasswordChangeRequired(user, DateTime.UtcNow))
    {
      await next(context);
      return;
    }

    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(
      Result.Forbidden("Your password has expired. Set a new password to continue."),
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

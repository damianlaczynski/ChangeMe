using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class AuthSessionUtils
{
  public const string InvalidCredentialsMessage = "Invalid email or password.";
  public const string DeactivatedAccountMessage = "This account has been deactivated. Contact an administrator.";
  public const string InvitePendingAccountMessage =
    "Complete your account setup using the invitation link sent to your email.";
  public const string DuplicateEmailMessage = "An account with this email already exists.";
  public const string InvalidInvitationTokenMessage =
    "This invitation link is invalid or has expired. Contact your administrator.";
  public const string InvitationAlreadyAcceptedMessage = "This account has already been activated.";
  public const string ForgotPasswordSuccessMessage =
    "If an account exists for this email, a reset link has been sent.";
  public const string InvalidPasswordResetTokenMessage =
    "This reset link is invalid or has expired. Request a new link from the sign-in page.";
  public const string PermissionDeniedMessage = "You do not have permission to perform this action.";
  public const string EmailNotVerifiedMessage = "Verify your email before signing in.";
  public const string RegistrationDisabledMessage =
    "Registration is disabled. Contact an administrator.";
  public const string EmailVerificationResendAckMessage =
    "If an unverified account exists for this email, a verification link has been sent.";
  public const string InvalidEmailVerificationTokenMessage =
    "This verification link is invalid or has expired.";

  public static async Task<Result<AuthResponseDto>> CreateAuthResponseAsync(
    ApplicationDbContext context,
    IJwtTokenGenerator jwtTokenGenerator,
    User user,
    UserSession session,
    string refreshToken,
    bool passwordChangeRequired,
    DateTime? passwordExpiresAtUtc,
    bool twoFactorSetupRequired,
    CancellationToken cancellationToken,
    bool passkeySetupRequired = false)
  {
    var permissions = await PermissionResolver.GetEffectivePermissionsAsync(context, user.Id, cancellationToken);
    var accessToken = jwtTokenGenerator.GenerateToken(user, session.Id, permissions);

    return Result.Success(new AuthResponseDto(
      user.Id,
      user.FirstName,
      user.LastName,
      user.Email,
      session.Id,
      accessToken.Token,
      accessToken.ExpiresAtUtc,
      refreshToken,
      session.RefreshTokenExpiresAtUtc,
      permissions,
      passwordChangeRequired,
      passwordExpiresAtUtc,
      twoFactorSetupRequired,
      passkeySetupRequired));
  }

  public static string? GetClientIpAddress(HttpContext? httpContext)
  {
    if (httpContext is null)
      return null;

    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(forwardedFor))
      return forwardedFor.Split(',')[0].Trim();

    return httpContext.Connection.RemoteIpAddress?.ToString();
  }
}

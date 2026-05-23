using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Email;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class AuthEmailService(
  IEmailService emailService,
  IOptions<AuthOptions> authOptions,
  ILogger<AuthEmailService> logger) : IAuthEmailService
{
  private readonly AuthOptions settings = authOptions.Value;

  public Task<Result> SendAccountInvitationAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "You're invited to ChangeMe",
      BrandedEmailTemplates.BuildActionEmail(
        "You're invited",
        "You have been invited to ChangeMe.",
        "Activate your account to set your password and sign in.",
        BuildLink("/accept-invitation", plainToken),
        "Activate account"),
      cancellationToken);

  public Task<Result> SendPasswordResetRequestedAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Reset your ChangeMe password",
      BrandedEmailTemplates.BuildActionEmail(
        "Reset your password",
        "A password reset was requested for your ChangeMe account.",
        "Use the button below to choose a new password. If you did not request this, you can ignore this email.",
        BuildLink("/reset-password", plainToken),
        "Reset password"),
      cancellationToken);

  public Task<Result> SendPasswordResetCompletedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Your password was changed",
      BrandedEmailTemplates.BuildActionEmail(
        "Password updated",
        "Your ChangeMe password was changed successfully.",
        "If you did not make this change, contact an administrator immediately.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendPasswordChangedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Your password was changed",
      BrandedEmailTemplates.BuildActionEmail(
        "Password updated",
        "Your ChangeMe password was changed.",
        "You have been signed out on all devices. Sign in again with your new password.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendVerifyEmailAsync(
    User user,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Verify your ChangeMe email",
      BrandedEmailTemplates.BuildActionEmail(
        "Verify your email",
        "Please verify your email address to use ChangeMe.",
        "Click the button below to confirm that you own this email address.",
        BuildLink("/verify-email", plainToken),
        "Verify email"),
      cancellationToken);

  private async Task<Result> SendAsync(
    string to,
    string subject,
    string body,
    CancellationToken cancellationToken)
  {
    var result = await emailService.SendEmailAsync(to, subject, body);

    if (!result.IsSuccess)
      logger.LogWarning("Auth email delivery failed for {Subject} to {Recipient}", subject, to);

    return result;
  }

  private string BuildLink(string path, string? plainToken)
  {
    var baseUrl = settings.FrontendBaseUrl.TrimEnd('/');
    return plainToken is null
      ? $"{baseUrl}{path}"
      : $"{baseUrl}{path}?token={Uri.EscapeDataString(plainToken)}";
  }
}

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

  public Task<Result> SendTwoFactorEnabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Two-factor authentication enabled",
      BrandedEmailTemplates.BuildActionEmail(
        "Two-factor authentication enabled",
        "Two-factor authentication was enabled on your ChangeMe account.",
        "You will need an authenticator app code when signing in with your password.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendTwoFactorDisabledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Two-factor authentication disabled",
      BrandedEmailTemplates.BuildActionEmail(
        "Two-factor authentication disabled",
        "Two-factor authentication was disabled on your ChangeMe account.",
        "If this was not you, contact an administrator immediately.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendTwoFactorResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Two-factor authentication was reset",
      BrandedEmailTemplates.BuildActionEmail(
        "Two-factor authentication reset",
        "An administrator reset two-factor authentication on your ChangeMe account.",
        "Sign in and set up two-factor authentication again if required.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendRecoveryCodeUsedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "A recovery code was used on your account",
      BrandedEmailTemplates.BuildActionEmail(
        "Recovery code used",
        "A recovery code was used to sign in to your ChangeMe account.",
        "If this was not you, reset your password and contact an administrator.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);

  public Task<Result> SendExternalAccountLinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "External sign-in method linked",
      BrandedEmailTemplates.BuildActionEmail(
        "External sign-in linked",
        $"{providerDisplayName} was linked to your ChangeMe account.",
        "You can now sign in with this provider.",
        BuildLink("/account", null),
        "My account"),
      cancellationToken);

  public Task<Result> SendExternalAccountUnlinkedAsync(
    User user,
    string providerDisplayName,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "External sign-in method removed",
      BrandedEmailTemplates.BuildActionEmail(
        "External sign-in removed",
        $"{providerDisplayName} was removed from your ChangeMe account.",
        "If this was not you, contact an administrator.",
        BuildLink("/account", null),
        "My account"),
      cancellationToken);

  public Task<Result> SendPasskeyAddedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default)
  {
    var eventTimeUtc = DateTime.UtcNow;
    return SendAsync(
      user.Email,
      "Passkey added to your account",
      BrandedEmailTemplates.BuildAuthEventEmail(
        "Passkey added",
        $"A passkey named \"{passkeyName}\" was added to your ChangeMe account.",
        user.Email,
        eventTimeUtc,
        "If you did not perform this action, contact your administrator immediately.",
        BuildLink("/account", null),
        "My account",
        passkeyName),
      cancellationToken);
  }

  public Task<Result> SendPasskeyRemovedAsync(
    User user,
    string passkeyName,
    CancellationToken cancellationToken = default)
  {
    var eventTimeUtc = DateTime.UtcNow;
    return SendAsync(
      user.Email,
      "Passkey removed from your account",
      BrandedEmailTemplates.BuildAuthEventEmail(
        "Passkey removed",
        $"Passkey \"{passkeyName}\" was removed from your ChangeMe account.",
        user.Email,
        eventTimeUtc,
        "If you did not perform this action, contact your administrator immediately.",
        BuildLink("/account", null),
        "My account",
        passkeyName),
      cancellationToken);
  }

  public Task<Result> SendPasskeysResetByAdminAsync(
    User user,
    CancellationToken cancellationToken = default)
  {
    var eventTimeUtc = DateTime.UtcNow;
    return SendAsync(
      user.Email,
      "Passkeys reset on your account",
      BrandedEmailTemplates.BuildAuthEventEmail(
        "Passkeys reset",
        "An administrator removed all passkeys from your ChangeMe account.",
        user.Email,
        eventTimeUtc,
        "If you did not expect this, contact your administrator immediately.",
        BuildLink("/login", null),
        "Sign in"),
      cancellationToken);
  }

  public Task<Result> SendEmailChangeRequestedAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Email change requested on your account",
      BrandedEmailTemplates.BuildActionEmail(
        "Email change requested",
        "A change to the email address on your ChangeMe account was requested.",
        "Your current email stays active for sign-in until you confirm the change from the new mailbox.",
        BuildLink("/account", null),
        "My account"),
      cancellationToken);

  public Task<Result> SendConfirmEmailChangeAsync(
    string newEmail,
    string plainToken,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      newEmail,
      "Confirm your new ChangeMe email address",
      BrandedEmailTemplates.BuildActionEmail(
        "Confirm your new email",
        "Confirm this email address to complete your ChangeMe account email change.",
        "If you did not request this change, you can ignore this email.",
        BuildLink("/confirm-email-change", plainToken),
        "Confirm email"),
      cancellationToken);

  public Task<Result> SendEmailChangeCancelledAsync(
    User user,
    CancellationToken cancellationToken = default) =>
    SendAsync(
      user.Email,
      "Email change cancelled",
      BrandedEmailTemplates.BuildActionEmail(
        "Email change cancelled",
        "The pending email change on your ChangeMe account was cancelled.",
        "Your current email address is unchanged.",
        BuildLink("/account", null),
        "My account"),
      cancellationToken);

  public Task<Result> SendEmailChangeCompletedAsync(
    string previousEmail,
    string newEmail,
    CancellationToken cancellationToken = default) =>
    SendEmailToBothAsync(
      previousEmail,
      newEmail,
      "Your ChangeMe email address was changed",
      "Email address changed",
      "The email address on your ChangeMe account was changed.",
      "Sign in with your new email address. You have been signed out on all devices.",
      cancellationToken);

  public Task<Result> SendEmailChangedByAdminAsync(
    string previousEmail,
    string newEmail,
    CancellationToken cancellationToken = default) =>
    SendEmailToBothAsync(
      previousEmail,
      newEmail,
      "Your ChangeMe email address was changed by an administrator",
      "Email changed by administrator",
      "An administrator changed the email address on your ChangeMe account.",
      "Sign in with your new email address. You have been signed out on all devices.",
      cancellationToken);

  private async Task<Result> SendEmailToBothAsync(
    string previousEmail,
    string newEmail,
    string subject,
    string title,
    string lead,
    string detail,
    CancellationToken cancellationToken)
  {
    var body = BrandedEmailTemplates.BuildActionEmail(
      title,
      lead,
      detail,
      BuildLink("/login", null),
      "Sign in");

    var first = await SendAsync(previousEmail, subject, body, cancellationToken);
    if (!first.IsSuccess)
      return first;

    return await SendAsync(newEmail, subject, body, cancellationToken);
  }

  private async Task<Result> SendAsync(
    string to,
    string subject,
    string body,
    CancellationToken cancellationToken)
  {
    var result = await emailService.SendEmailAsync(to, subject, body);

    if (!result.IsSuccess)
    {
      logger.LogWarning("Auth email delivery failed for {Subject} to {Recipient}", subject, to);
      return Result.Error("The email could not be sent. Please try again.");
    }

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

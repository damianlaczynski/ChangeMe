using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class ExternalAuthUtils
{
  public const string ExternalSignInFailedMessage =
    "External sign-in failed. Try again or use email and password.";

  public const string SignInNotAllowedMessage = "Sign-in with this account is not allowed.";

  public const string NoAccountExistsMessage =
    "No account exists for this email. Contact an administrator.";

  public const string ExternalAccountAlreadyLinkedMessage =
    "This external account is already linked to another user.";

  public const string ExternalProvidersDisabledMessage =
    "External sign-in is unavailable. Contact an administrator or set a password when sign-in is available.";

  public const string ExternalStepUpRequiredMessage =
    "Complete sign-in with a linked external provider to continue.";

  public const string CannotRemoveOnlySignInMethodMessage =
    "Set a password before removing your only sign-in method.";

  public const string ExternalAccountLinkedMessage = "External sign-in method linked.";

  public const string ExternalAccountUnlinkedMessage = "External sign-in method removed.";

  public const string ExternalProviderEmailMismatchMessage =
    "The external account email does not match your account email.";

  public static bool IsInvitationPending(User user) => user.HasPendingInvitation;

  public static bool IsExternalOnlyAccount(User user) =>
    !user.HasPasswordSet && !user.HasPendingInvitation && user.ExternalLogins.Count > 0;

  public static bool IsExternalStepUpFresh(User user, AuthOptions auth, DateTime utcNow)
  {
    if (user.HasPasswordSet || user.ExternalLogins.Count == 0)
      return true;

    var stepUpThreshold = utcNow.AddMinutes(-auth.TwoFactor.StepUpExternalSignInValidityMinutes);
    return user.ExternalLogins.Any(x => x.LastStepUpAtUtc >= stepUpThreshold);
  }

  public static string BuildRedirectUri(AuthOptions auth) =>
    $"{auth.FrontendBaseUrl.TrimEnd('/')}{auth.External.SignInCallbackPath}";

  public static ExternalProviderConfiguration? ResolveProvider(AuthOptions auth, string providerKey)
  {
    var provider = auth.External.Providers
      .FirstOrDefault(x =>
        x.IsConfigured
        && x.ProviderKey.Equals(providerKey.Trim(), StringComparison.OrdinalIgnoreCase));

    return provider is null
      ? null
      : new ExternalProviderConfiguration(
        provider.ProviderKey,
        provider.DisplayName,
        provider.Authority,
        provider.ClientId,
        provider.ClientSecret,
        provider.AllowedEmailDomains,
        provider.TrustIdpEmailWithoutEmailVerified,
        provider.IssuerValidationMode);
  }

  public static bool ProviderEmailMatchesUser(User user, string? providerEmail, bool providerEmailVerified) =>
    providerEmailVerified
    && !string.IsNullOrWhiteSpace(providerEmail)
    && user.NormalizedEmail == User.NormalizeEmail(providerEmail);

  public static bool IsEmailDomainAllowed(string? email, IReadOnlyList<string> allowedDomains)
  {
    if (allowedDomains.Count == 0)
      return true;

    if (string.IsNullOrWhiteSpace(email))
      return false;

    var normalizedEmail = User.NormalizeEmail(email);
    return allowedDomains.Any(domain =>
    {
      var normalizedDomain = domain.Trim().TrimStart('@').ToLowerInvariant();
      return normalizedEmail.EndsWith(
        "@" + normalizedDomain,
        StringComparison.OrdinalIgnoreCase);
    });
  }

  public static bool IsTwoFactorVerificationRequired(
    User user,
    AuthOptions auth,
    bool identityProviderMfaAsserted) =>
    auth.TwoFactor.Enabled
    && user.TwoFactorEnabled
    && !(auth.TwoFactor.TrustIdentityProviderMfa && identityProviderMfaAsserted);

  public static bool IsTwoFactorSetupRequired(
    User user,
    AuthOptions auth,
    bool identityProviderMfaAsserted)
  {
    if (!auth.TwoFactor.Enabled || !auth.TwoFactor.Required)
      return false;

    if (user.TwoFactorEnabled)
      return false;

    if (auth.TwoFactor.TrustIdentityProviderMfa && identityProviderMfaAsserted)
      return false;

    return true;
  }

  public static Result ValidateCanUnlinkExternalLogin(User user, string providerKey)
  {
    var hasLogin = user.ExternalLogins.Any(x =>
      x.ProviderKey.Equals(providerKey.Trim(), StringComparison.OrdinalIgnoreCase));
    if (!hasLogin)
      return Result.NotFound();

    if (!user.HasPasswordSet && user.ExternalLogins.Count <= 1)
      return Result.Error(CannotRemoveOnlySignInMethodMessage);

    return Result.Success();
  }

  public static string ResolveProviderDisplayName(AuthOptions auth, string providerKey) =>
    auth.External.Providers
      .FirstOrDefault(x =>
        x.ProviderKey.Equals(providerKey, StringComparison.OrdinalIgnoreCase))
      ?.DisplayName
    ?? providerKey;

  public static async Task DeletePendingAsync(
    ApplicationDbContext context,
    ExternalAuthPending pending,
    CancellationToken cancellationToken)
  {
    await context.ExternalAuthPending
      .Where(x => x.Id == pending.Id)
      .ExecuteDeleteAsync(cancellationToken);

    context.Entry(pending).State = EntityState.Detached;
  }
}

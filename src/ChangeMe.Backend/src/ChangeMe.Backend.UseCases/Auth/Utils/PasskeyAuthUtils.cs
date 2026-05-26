using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class PasskeyAuthUtils
{
  public const string NotSupportedMessage =
    "Passkeys are not supported in this browser. Use email and password or try another browser.";
  public const string TimedOutMessage = "Passkey sign-in timed out. Try again.";
  public const string NoMatchMessage =
    "No passkey matched. Sign in with email and password or use a different passkey.";
  public const string NoPasskeyForAccountMessage = "No passkey is registered for this account.";
  public const string PasskeyOnlyNotAllowedMessage =
    "Set a password or use external sign-in before using passkeys on this account.";
  public const string TooManyAttemptsMessage = "Too many passkey attempts. Try again.";
  public const string VerificationFailedMessage = "Passkey verification failed. Try again.";
  public const string MaximumPasskeysMessage =
    "Maximum number of passkeys reached. Remove one before adding another.";
  public const string RemoveOnlySignInMethodMessage =
    "Add a password or external sign-in before removing your only sign-in method.";
  public const string RemoveRequiredPasskeyMessage =
    "At least one passkey is required. Add another passkey before removing this one.";
  public const string PasskeyStepUpRequiredMessage =
    "Verify your identity with a passkey to continue.";

  public static bool DoesCeremonyEmailMatchUser(WebAuthnCeremonyPending ceremony, User user) =>
    string.IsNullOrEmpty(ceremony.NormalizedEmail)
    || user.NormalizedEmail == ceremony.NormalizedEmail;

  public static bool CanUsePasskeySignIn(
    User user,
    int passkeyCount,
    AuthOptions auth)
  {
    if (!auth.Passkeys.PasskeysAuthenticationEnabled || passkeyCount == 0)
      return false;

    if (user.HasPasswordSet || user.ExternalLogins.Count > 0)
      return true;

    return auth.Passkeys.AllowPasskeyOnlyAccounts;
  }

  public static bool IsTwoFactorVerificationRequiredAfterPasskey(
    User user,
    AuthOptions auth,
    IPasskeyPolicyEvaluator passkeyPolicy,
    bool userVerificationPresent) =>
    user.TwoFactorEnabled
    && auth.TwoFactor.Enabled
    && !passkeyPolicy.DoesPasskeySatisfyTwoFactor(userVerificationPresent);

  public static bool IsTwoFactorSetupRequiredAfterPasskey(
    User user,
    AuthOptions auth,
    IPasskeyPolicyEvaluator passkeyPolicy,
    bool userVerificationPresent) =>
    auth.TwoFactor.Enabled
    && auth.TwoFactor.Required
    && !user.TwoFactorEnabled
    && user.HasPasswordSet
    && !passkeyPolicy.DoesPasskeySatisfyTwoFactor(userVerificationPresent);

  public static async Task<Result<LoginResponseDto>> IssuePasskeySignInResponseAsync(
    ApplicationDbContext context,
    IJwtTokenGenerator jwtTokenGenerator,
    ISessionLifetimeService sessionLifetime,
    IPasswordExpirationEvaluator passwordExpirationEvaluator,
    ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
    IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AuthOptions> authOptions,
    User user,
    bool userVerificationPresent,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    var utcNow = DateTime.UtcNow;
    var passwordChangeRequired = passwordExpirationEvaluator.IsPasswordChangeRequired(user, utcNow);

    if (!passwordChangeRequired
        && IsTwoFactorVerificationRequiredAfterPasskey(user, auth, passkeyPolicyEvaluator, userVerificationPresent))
    {
      var challengeResult = await CreateSignInChallengeAsync(
        context,
        auth,
        user,
        utcNow,
        SignInMethods.Passkey,
        cancellationToken);
      if (!challengeResult.IsSuccess)
        return challengeResult.Map();

      return Result.Success(new LoginResponseDto
      {
        TwoFactorChallenge = new PendingSignInChallengeDto(challengeResult.Value.Id)
      });
    }

    var sessionResult = await AuthSessionFactory.CreateSessionAsync(
      context,
      sessionLifetime,
      httpContextAccessor,
      user,
      cancellationToken,
      SignInMethods.Passkey);
    if (!sessionResult.IsSuccess)
      return Result<LoginResponseDto>.Invalid(sessionResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == user.Id, cancellationToken);
    var passwordExpiresAtUtc = passwordExpirationEvaluator.GetPasswordExpiresAtUtc(user);
    var twoFactorSetupRequired = !passwordChangeRequired
      && IsTwoFactorSetupRequiredAfterPasskey(user, auth, passkeyPolicyEvaluator, userVerificationPresent);
    var passkeySetupRequired = !passwordChangeRequired
      && !twoFactorSetupRequired
      && passkeyPolicyEvaluator.IsPasskeySetupRequired(user, passkeyCount);

    var authResponse = await AuthSessionUtils.CreateAuthResponseAsync(
      context,
      jwtTokenGenerator,
      user,
      sessionResult.Value.Session,
      sessionResult.Value.RefreshToken,
      passwordChangeRequired,
      passwordExpiresAtUtc,
      twoFactorSetupRequired,
      cancellationToken,
      passkeySetupRequired);

    if (!authResponse.IsSuccess)
      return authResponse.Map();

    return Result.Success(new LoginResponseDto { AuthSession = authResponse.Value });
  }

  private static async Task<Result<SignInChallenge>> CreateSignInChallengeAsync(
    ApplicationDbContext context,
    AuthOptions auth,
    User user,
    DateTime utcNow,
    string pendingSignInMethod,
    CancellationToken cancellationToken)
  {
    var expiresAtUtc = utcNow.AddMinutes(auth.TwoFactor.PendingSignInChallengeLifetimeMinutes);
    var challengeResult = SignInChallenge.Create(user.Id, expiresAtUtc, pendingSignInMethod);
    if (!challengeResult.IsSuccess)
      return challengeResult.Map();

    await context.SignInChallenges
      .Where(x => x.UserId == user.Id)
      .ExecuteDeleteAsync(cancellationToken);

    await context.SignInChallenges.AddAsync(challengeResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    return challengeResult;
  }
}

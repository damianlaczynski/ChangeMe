using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class PasskeyAuthUtilsTests
{
  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenCeremonyHasNoEmail_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5)).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.True(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenEmailsMatch_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5),
      normalizedEmail: User.NormalizeEmail("user@example.com")).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.True(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenEmailsDiffer_ShouldReturnFalse()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5),
      normalizedEmail: User.NormalizeEmail("other@example.com")).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.False(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void IsAttemptLimitReached_WhenCountEqualsMax_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.StepUp,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5)).Value;

    for (var i = 0; i < 5; i++)
      ceremony.RecordFailedAttempt();

    Assert.True(PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, 5));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenPasskeyOnlyAndNotAllowed_ShouldReturnFalse()
  {
    var user = User.CreateInvited("user@example.com").Value;
    var auth = CreateAuthOptions(allowPasskeyOnlyAccounts: false);

    Assert.False(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 1, auth));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenPasskeyOnlyAndAllowed_ShouldReturnTrue()
  {
    var user = User.CreateInvited("user@example.com").Value;
    var auth = CreateAuthOptions(allowPasskeyOnlyAccounts: true);

    Assert.True(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 1, auth));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenPasswordSetAndPasskeyExists_ShouldReturnTrue()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    var auth = CreateAuthOptions(allowPasskeyOnlyAccounts: false);

    Assert.True(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 1, auth));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenExternalLoginExistsAndPasskeyExists_ShouldReturnTrue()
  {
    var user = User.CreateInvited("user@example.com").Value;
    user.AddExternalLogin(ExternalLogin.Create(user.Id, "google", "subject-1").Value);
    var auth = CreateAuthOptions(allowPasskeyOnlyAccounts: false);

    Assert.True(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 1, auth));
  }

  [Fact]
  public void IsTwoFactorVerificationRequiredAfterPasskey_WhenSatisfiesTwoFactorWithUv_ShouldReturnFalse()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    user.EnableTwoFactor("secret", DateTime.UtcNow);
    var auth = CreateAuthOptions(passkeySatisfiesTwoFactor: true, twoFactorEnabled: true);
    var evaluator = new PasskeyPolicyEvaluator(Options.Create(auth));

    Assert.False(PasskeyAuthUtils.IsTwoFactorVerificationRequiredAfterPasskey(
      user,
      auth,
      evaluator,
      userVerificationPresent: true));
  }

  [Fact]
  public void IsTwoFactorVerificationRequiredAfterPasskey_WhenUvMissing_ShouldReturnTrue()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    user.EnableTwoFactor("secret", DateTime.UtcNow);
    var auth = CreateAuthOptions(passkeySatisfiesTwoFactor: true, twoFactorEnabled: true);
    var evaluator = new PasskeyPolicyEvaluator(Options.Create(auth));

    Assert.True(PasskeyAuthUtils.IsTwoFactorVerificationRequiredAfterPasskey(
      user,
      auth,
      evaluator,
      userVerificationPresent: false));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenPasskeysDisabled_ShouldReturnFalse()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    var auth = CreateAuthOptions();
    auth.Passkeys.PasskeysAuthenticationEnabled = false;

    Assert.False(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 1, auth));
  }

  [Fact]
  public void CanUsePasskeySignIn_WhenNoPasskeysRegistered_ShouldReturnFalse()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    var auth = CreateAuthOptions();

    Assert.False(PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount: 0, auth));
  }

  [Fact]
  public void IsTwoFactorSetupRequiredAfterPasskey_WhenSatisfiesTwoFactorWithUv_ShouldReturnFalse()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    var auth = CreateAuthOptions(passkeySatisfiesTwoFactor: true, twoFactorEnabled: true);
    auth.TwoFactor.Required = true;
    var evaluator = new PasskeyPolicyEvaluator(Options.Create(auth));

    Assert.False(PasskeyAuthUtils.IsTwoFactorSetupRequiredAfterPasskey(
      user,
      auth,
      evaluator,
      userVerificationPresent: true));
  }

  [Fact]
  public void IsTwoFactorSetupRequiredAfterPasskey_WhenUvMissing_ShouldReturnTrue()
  {
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    var auth = CreateAuthOptions(passkeySatisfiesTwoFactor: true, twoFactorEnabled: true);
    auth.TwoFactor.Required = true;
    var evaluator = new PasskeyPolicyEvaluator(Options.Create(auth));

    Assert.True(PasskeyAuthUtils.IsTwoFactorSetupRequiredAfterPasskey(
      user,
      auth,
      evaluator,
      userVerificationPresent: false));
  }

  private static AuthOptions CreateAuthOptions(
    bool allowPasskeyOnlyAccounts = false,
    bool passkeySatisfiesTwoFactor = false,
    bool twoFactorEnabled = false) =>
    new()
    {
      Passkeys = new PasskeyOptions
      {
        PasskeysAuthenticationEnabled = true,
        AllowPasskeyOnlyAccounts = allowPasskeyOnlyAccounts,
        PasskeySatisfiesTwoFactor = passkeySatisfiesTwoFactor
      },
      TwoFactor = new TwoFactorOptions
      {
        Enabled = twoFactorEnabled
      }
    };
}

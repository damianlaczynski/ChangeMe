using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UnitTests.Support;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class TwoFactorStepUpUtilsTests
{
  private static readonly AuthOptions DefaultAuth = new()
  {
    Passkeys = new PasskeyOptions { PasskeysAuthenticationEnabled = true, PasskeyStepUpValidityMinutes = 15 },
    TwoFactor = new TwoFactorOptions { StepUpExternalSignInValidityMinutes = 15 }
  };

  [Fact]
  public void ValidateStepUp_WhenPasswordUserHasFreshPasskeyStepUp_ShouldSucceedWithoutPassword()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;
    user.RecordPasskeyStepUp(utcNow);

    var result = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      currentPassword: null,
      verificationCode: null,
      new FakePasswordHasher(valid: false),
      new FakeTotpService(),
      new FakeTwoFactorSecretProtector(),
      new FakeRecoveryCodeHasher(),
      Options.Create(DefaultAuth),
      passkeysEnabled: true,
      passkeyCount: 1,
      utcNow,
      out _);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ValidateStepUp_WhenExternalOnlyHasFreshPasskeyStepUp_ShouldSucceed()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("external@example.com").Value;
    user.AddExternalLogin(ExternalLogin.Create(user.Id, "google", "subject").Value);
    user.RecordPasskeyStepUp(utcNow);

    var result = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      currentPassword: null,
      verificationCode: null,
      new FakePasswordHasher(),
      new FakeTotpService(),
      new FakeTwoFactorSecretProtector(),
      new FakeRecoveryCodeHasher(),
      Options.Create(DefaultAuth),
      passkeysEnabled: true,
      passkeyCount: 1,
      utcNow,
      out _);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ValidateStepUp_WhenPasskeyOnlyWithoutFreshStepUp_ShouldFail()
  {
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("passkey-only@example.com").Value;

    var result = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      currentPassword: null,
      verificationCode: null,
      new FakePasswordHasher(),
      new FakeTotpService(),
      new FakeTwoFactorSecretProtector(),
      new FakeRecoveryCodeHasher(),
      Options.Create(DefaultAuth),
      passkeysEnabled: true,
      passkeyCount: 1,
      utcNow,
      out _);

    Assert.False(result.IsSuccess);
  }
}

internal sealed class FakePasswordHasher(bool valid = true) : IPasswordHasher
{
  public string HashPassword(string password) => $"hash:{password}";
  public bool VerifyPassword(string passwordHash, string password) => valid;
}

internal sealed class FakeTotpService : ITotpService
{
  public string GenerateSecret() => "secret";
  public string BuildProvisioningUri(string secret, string accountName) => $"otpauth://totp/{accountName}?secret={secret}";
  public bool ValidateCode(string secret, string code, DateTime utcNow) => code == "123456";
}

internal sealed class FakeTwoFactorSecretProtector : ITwoFactorSecretProtector
{
  public string Protect(string plaintextSecret) => plaintextSecret;
  public string Unprotect(string ciphertext) => ciphertext;
}

internal sealed class FakeRecoveryCodeHasher : IRecoveryCodeHasher
{
  public string Hash(string recoveryCode) => recoveryCode;
  public bool Verify(string recoveryCode, string codeHash) => recoveryCode == codeHash;
}

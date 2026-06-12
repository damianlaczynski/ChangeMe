using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class TwoFactorAuthUtils
{
  public const string InvalidVerificationCodeMessage = "Invalid verification code.";
  public const string SignInTimedOutMessage = "Sign-in timed out. Try again.";
  public const string TooManyAttemptsMessage = "Too many attempts. Sign in again.";

  public static bool LooksLikeTotpCode(string code, int verificationCodeLength)
  {
    var normalized = code.Trim();
    return normalized.Length == verificationCodeLength && normalized.All(char.IsDigit);
  }

  public static bool TryValidateVerificationCode(
    User user,
    string verificationCode,
    ITotpService totpService,
    ITwoFactorSecretProtector secretProtector,
    IRecoveryCodeHasher recoveryCodeHasher,
    IOptions<AuthOptions> authOptions,
    DateTime utcNow,
    out UserRecoveryCode? consumedRecoveryCode)
  {
    consumedRecoveryCode = null;
    var options = authOptions.Value.TwoFactor;

    if (LooksLikeTotpCode(verificationCode, options.VerificationCodeLength))
    {
      if (string.IsNullOrWhiteSpace(user.TwoFactorSecretCiphertext))
        return false;

      var secret = secretProtector.Unprotect(user.TwoFactorSecretCiphertext);
      return totpService.ValidateCode(secret, verificationCode, utcNow);
    }

    foreach (var recoveryCode in user.RecoveryCodes.Where(x => !x.IsUsed))
    {
      if (!recoveryCodeHasher.Verify(verificationCode, recoveryCode.CodeHash))
        continue;

      consumedRecoveryCode = recoveryCode;
      return true;
    }

    return false;
  }
}

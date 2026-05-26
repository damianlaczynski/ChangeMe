using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth.Utils;

public static class TwoFactorStepUpUtils
{
  public static Result ValidateStepUp(
    User user,
    string? currentPassword,
    string? verificationCode,
    IPasswordHasher passwordHasher,
    ITotpService totpService,
    ITwoFactorSecretProtector secretProtector,
    IRecoveryCodeHasher recoveryCodeHasher,
    IOptions<AuthOptions> authOptions,
    bool passkeysEnabled,
    int passkeyCount,
    DateTime utcNow,
    out UserRecoveryCode? consumedRecoveryCode)
  {
    consumedRecoveryCode = null;
    var validationErrors = new List<ValidationError>();
    var auth = authOptions.Value;
    var passkeyStepUpFresh = IsPasskeyStepUpFresh(user, auth, passkeysEnabled, passkeyCount, utcNow);

    if (user.HasPasswordSet)
    {
      var passwordValid = !string.IsNullOrWhiteSpace(currentPassword)
        && passwordHasher.VerifyPassword(user.PasswordHash, currentPassword);

      if (!passwordValid && !passkeyStepUpFresh)
      {
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
          validationErrors.Add(new ValidationError(
            nameof(currentPassword),
            "Current password is required."));
        }
        else
        {
          validationErrors.Add(new ValidationError(
            nameof(currentPassword),
            "Current password is incorrect."));
        }
      }
    }
    else if (user.ExternalLogins.Count > 0)
    {
      if (!ExternalAuthUtils.IsExternalStepUpFresh(user, auth, utcNow) && !passkeyStepUpFresh)
        return Result.Error(ExternalAuthUtils.ExternalStepUpRequiredMessage);
    }
    else if (passkeysEnabled && passkeyCount > 0)
    {
      if (!passkeyStepUpFresh)
        return Result.Error(PasskeyAuthUtils.PasskeyStepUpRequiredMessage);
    }

    if (user.TwoFactorEnabled)
    {
      if (string.IsNullOrWhiteSpace(verificationCode))
      {
        validationErrors.Add(new ValidationError(
          nameof(verificationCode),
          "Verification code is required."));
      }
      else if (!TwoFactorAuthUtils.TryValidateVerificationCode(
                 user,
                 verificationCode,
                 totpService,
                 secretProtector,
                 recoveryCodeHasher,
                 authOptions,
                 utcNow,
                 out consumedRecoveryCode))
      {
        validationErrors.Add(new ValidationError(
          nameof(verificationCode),
          TwoFactorAuthUtils.InvalidVerificationCodeMessage));
      }
    }

    if (validationErrors.Count > 0)
      return Result.Invalid(validationErrors);

    return Result.Success();
  }

  public static async Task<(Result Result, UserRecoveryCode? ConsumedRecoveryCode)> ValidateSignedInStepUpAsync(
    ApplicationDbContext context,
    User user,
    Guid userId,
    string? currentPassword,
    string? verificationCode,
    IPasswordHasher passwordHasher,
    ITotpService totpService,
    ITwoFactorSecretProtector secretProtector,
    IRecoveryCodeHasher recoveryCodeHasher,
    IOptions<AuthOptions> authOptions,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var auth = authOptions.Value;
    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    var stepUpResult = ValidateStepUp(
      user,
      currentPassword,
      verificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      auth.Passkeys.PasskeysAuthenticationEnabled,
      passkeyCount,
      utcNow,
      out var consumedRecoveryCode);

    return (stepUpResult, consumedRecoveryCode);
  }

  public static bool IsPasskeyStepUpFresh(
    User user,
    AuthOptions auth,
    bool passkeysEnabled,
    int passkeyCount,
    DateTime utcNow) =>
    passkeysEnabled
    && passkeyCount > 0
    && user.IsPasskeyStepUpFresh(utcNow, auth.Passkeys.PasskeyStepUpValidityMinutes);
}

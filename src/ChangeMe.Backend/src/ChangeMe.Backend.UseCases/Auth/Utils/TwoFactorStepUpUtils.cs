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
    DateTime utcNow,
    out UserRecoveryCode? consumedRecoveryCode)
  {
    consumedRecoveryCode = null;
    var validationErrors = new List<ValidationError>();

    if (user.HasPasswordSet)
    {
      if (string.IsNullOrWhiteSpace(currentPassword))
      {
        validationErrors.Add(new ValidationError(
          nameof(currentPassword),
          "Current password is required."));
      }
      else if (!passwordHasher.VerifyPassword(user.PasswordHash, currentPassword))
      {
        validationErrors.Add(new ValidationError(
          nameof(currentPassword),
          "Current password is incorrect."));
      }
    }
    else if (user.ExternalLogins.Count > 0)
    {
      if (!ExternalAuthUtils.IsExternalStepUpFresh(user, authOptions.Value, utcNow))
      {
        return Result.Error(ExternalAuthUtils.ExternalStepUpRequiredMessage);
      }
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
}

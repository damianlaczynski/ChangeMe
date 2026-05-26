using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using Microsoft.Extensions.Options;
using OtpNet;

namespace ChangeMe.Backend.Infrastructure.Auth;

public sealed class TotpService(IOptions<AuthOptions> authOptions) : ITotpService
{
  public string GenerateSecret()
  {
    var key = KeyGeneration.GenerateRandomKey(20);
    return Base32Encoding.ToString(key);
  }

  public string BuildProvisioningUri(string secret, string accountName)
  {
    var options = authOptions.Value.TwoFactor;
    var issuer = Uri.EscapeDataString(options.TotpIssuerName);
    var account = Uri.EscapeDataString(accountName);
    return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&digits={options.VerificationCodeLength}&period={options.TotpTimeStepSeconds}";
  }

  public bool ValidateCode(string secret, string code, DateTime utcNow)
  {
    if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
      return false;

    var normalizedCode = code.Trim();
    var options = authOptions.Value.TwoFactor;
    if (normalizedCode.Length != options.VerificationCodeLength || !normalizedCode.All(char.IsDigit))
      return false;

    var keyBytes = Base32Encoding.ToBytes(secret);
    var totp = new Totp(
      keyBytes,
      step: options.TotpTimeStepSeconds,
      totpSize: options.VerificationCodeLength);

    return totp.VerifyTotp(
      utcNow,
      normalizedCode,
      out _,
      new VerificationWindow(options.TotpValidationWindowSteps, options.TotpValidationWindowSteps));
  }
}

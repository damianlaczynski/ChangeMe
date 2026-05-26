using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class TotpServiceTests
{
  [Fact]
  public void ValidateCode_WhenWithinValidationWindow_ShouldAcceptAdjacentStep()
  {
    var options = Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        TotpTimeStepSeconds = 30,
        TotpValidationWindowSteps = 1,
        VerificationCodeLength = 6
      }
    });
    var service = new TotpService(options);
    var secret = service.GenerateSecret();
    var totp = new OtpNet.Totp(
      OtpNet.Base32Encoding.ToBytes(secret),
      step: 30,
      totpSize: 6);
    var previousStepCode = totp.ComputeTotp(DateTime.UtcNow.AddSeconds(-30));

    Assert.True(service.ValidateCode(secret, previousStepCode, DateTime.UtcNow));
  }

  [Fact]
  public void ValidateCode_WhenOutsideValidationWindow_ShouldRejectOldCode()
  {
    var options = Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        TotpTimeStepSeconds = 30,
        TotpValidationWindowSteps = 1,
        VerificationCodeLength = 6
      }
    });
    var service = new TotpService(options);
    var secret = service.GenerateSecret();
    var totp = new OtpNet.Totp(
      OtpNet.Base32Encoding.ToBytes(secret),
      step: 30,
      totpSize: 6);
    var oldCode = totp.ComputeTotp(DateTime.UtcNow.AddSeconds(-120));

    Assert.False(service.ValidateCode(secret, oldCode, DateTime.UtcNow));
  }

  [Fact]
  public void ValidateCode_WhenUtcNowDiffersFromSystemClock_ShouldUseProvidedTimestamp()
  {
    var options = Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        TotpTimeStepSeconds = 30,
        TotpValidationWindowSteps = 1,
        VerificationCodeLength = 6
      }
    });
    var service = new TotpService(options);
    var secret = service.GenerateSecret();
    var referenceTime = new DateTime(2020, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    var totp = new OtpNet.Totp(
      OtpNet.Base32Encoding.ToBytes(secret),
      step: 30,
      totpSize: 6);
    var code = totp.ComputeTotp(referenceTime);

    Assert.True(service.ValidateCode(secret, code, referenceTime));
    Assert.False(service.ValidateCode(secret, code, DateTime.UtcNow));
  }

  [Fact]
  public void ValidateCode_WhenWrongLength_ShouldReturnFalse()
  {
    var service = new TotpService(Options.Create(new AuthOptions()));
    var secret = service.GenerateSecret();

    Assert.False(service.ValidateCode(secret, "12345", DateTime.UtcNow));
  }
}

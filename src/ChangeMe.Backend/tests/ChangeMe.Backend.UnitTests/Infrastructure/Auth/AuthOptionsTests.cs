using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests;

public sealed class AuthOptionsTests
{
  [Fact]
  public void PasswordPolicyOptions_WhenCreated_ShouldUseRequirementDefaults()
  {
    var policy = new PasswordPolicyOptions();

    Assert.Equal(UserConstraints.PASSWORD_MIN_LENGTH, policy.MinimumLength);
    Assert.Equal(UserConstraints.PASSWORD_MAX_LENGTH, policy.MaximumLength);
    Assert.True(policy.RequireUppercase);
    Assert.True(policy.RequireLowercase);
    Assert.True(policy.RequireDigit);
    Assert.False(policy.RequireSpecialCharacter);
  }

  [Fact]
  public async Task GetAuthSettingsHandler_ShouldMapConfiguredOptions()
  {
    var options = Options.Create(new AuthOptions
    {
      Registration = new RegistrationOptions { PublicEnabled = false },
      EmailVerification = new EmailVerificationOptions { Enabled = true },
      PasswordExpiration = new PasswordExpirationOptions
      {
        Enabled = true,
        MaximumPasswordAgeDays = 60
      },
      PasswordPolicy = new PasswordPolicyOptions
      {
        MinimumLength = 10,
        MaximumLength = 64,
        RequireUppercase = false,
        RequireLowercase = true,
        RequireDigit = true,
        RequireSpecialCharacter = true
      }
    });

    var handler = new GetAuthSettingsHandler(options);
    var result = await handler.Handle(new GetAuthSettingsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(result.Value.PublicRegistrationEnabled);
    Assert.True(result.Value.EmailVerificationEnabled);
    Assert.True(result.Value.PasswordExpirationEnabled);
    Assert.Equal(60, result.Value.MaximumPasswordAgeDays);
    Assert.Equal(10, result.Value.PasswordPolicy.MinimumLength);
    Assert.Equal(64, result.Value.PasswordPolicy.MaximumLength);
    Assert.False(result.Value.PasswordPolicy.RequireUppercase);
    Assert.True(result.Value.PasswordPolicy.RequireSpecialCharacter);
  }

  [Fact]
  public void TwoFactorOptions_WhenCreated_ShouldUseRequirementDefaults()
  {
    var twoFactor = new TwoFactorOptions();

    Assert.Equal(30, twoFactor.TotpTimeStepSeconds);
    Assert.Equal(1, twoFactor.TotpValidationWindowSteps);
    Assert.Equal(6, twoFactor.VerificationCodeLength);
    Assert.Equal(10, twoFactor.RecoveryCodeCount);
    Assert.Equal(5, twoFactor.MaxFailedVerificationAttempts);
  }

  [Fact]
  public async Task GetAuthSettingsHandler_ShouldMapTwoFactorAndExternalProviderSettings()
  {
    var options = Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        Enabled = true,
        Required = true,
        TrustIdentityProviderMfa = true,
        VerificationCodeLength = 6,
        RecoveryCodeCount = 10,
        TotpTimeStepSeconds = 30
      },
      External = new ExternalAuthOptions
      {
        Enabled = true,
        Providers =
      [
        new ExternalProviderOptions
        {
          ProviderKey = "google",
          DisplayName = "Google",
          Authority = "https://accounts.google.com",
          ClientId = "client-id",
          ClientSecret = "secret"
        }
      ]
      }
    });

    var handler = new GetAuthSettingsHandler(options);
    var result = await handler.Handle(new GetAuthSettingsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.TwoFactorAuthenticationEnabled);
    Assert.True(result.Value.TwoFactorAuthenticationRequired);
    Assert.True(result.Value.TrustIdentityProviderMfa);
    Assert.True(result.Value.ExternalProvidersEnabled);
    Assert.Equal(6, result.Value.TwoFactor.VerificationCodeLength);
    Assert.Single(result.Value.ExternalProviders);
    Assert.Equal("google", result.Value.ExternalProviders[0].ProviderKey);
  }
}

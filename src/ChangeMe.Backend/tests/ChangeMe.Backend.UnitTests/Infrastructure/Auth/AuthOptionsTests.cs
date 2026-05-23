using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
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
      PublicRegistrationEnabled = false,
      EmailVerificationEnabled = true,
      PasswordExpirationEnabled = true,
      MaximumPasswordAgeDays = 60,
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
}

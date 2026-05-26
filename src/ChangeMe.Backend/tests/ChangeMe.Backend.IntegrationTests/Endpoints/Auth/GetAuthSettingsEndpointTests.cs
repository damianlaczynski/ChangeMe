using System.Net;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class GetAuthSettingsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAuthSettings_WhenAnonymous_ShouldReturnOkWithPolicyDefaults()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/api/auth/settings", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var settings = await IntegrationApiJson.ReadValueAsync<AuthSettingsDto>(response.Content, cancellationToken);
    Assert.NotNull(settings);
    Assert.True(settings.PublicRegistrationEnabled);
    Assert.False(settings.EmailVerificationEnabled);
    Assert.False(settings.PasswordExpirationEnabled);
    Assert.Equal(8, settings.PasswordPolicy.MinimumLength);
    Assert.Equal(128, settings.PasswordPolicy.MaximumLength);
    Assert.True(settings.PasswordPolicy.RequireUppercase);
    Assert.True(settings.PasswordPolicy.RequireLowercase);
    Assert.True(settings.PasswordPolicy.RequireDigit);
    Assert.False(settings.PasswordPolicy.RequireSpecialCharacter);
    Assert.False(settings.TwoFactorAuthenticationEnabled);
    Assert.False(settings.ExternalProvidersEnabled);
    Assert.Equal(6, settings.TwoFactor.VerificationCodeLength);
    Assert.Equal(10, settings.TwoFactor.RecoveryCodeCount);
    Assert.Equal(30, settings.TwoFactor.TotpTimeStepSeconds);
    Assert.Empty(settings.ExternalProviders);
  }
}

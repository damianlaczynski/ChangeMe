using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class AuthFeaturesWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides[$"{AuthOptions.SectionName}__EmailVerification__Enabled"] = "true";
    overrides[$"{AuthOptions.SectionName}__PasswordExpiration__Enabled"] = "true";
    overrides[$"{AuthOptions.SectionName}__PasswordExpiration__MaximumPasswordAgeDays"] = "90";
    overrides[$"{AuthOptions.SectionName}__Registration__PublicEnabled"] = "true";
  }
}

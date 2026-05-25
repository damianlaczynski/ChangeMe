namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class AuthFeaturesWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerification__Enabled"] = "true";
    overrides["Auth__PasswordExpiration__Enabled"] = "true";
    overrides["Auth__PasswordExpiration__MaximumPasswordAgeDays"] = "90";
    overrides["Auth__Registration__PublicEnabled"] = "true";
  }
}

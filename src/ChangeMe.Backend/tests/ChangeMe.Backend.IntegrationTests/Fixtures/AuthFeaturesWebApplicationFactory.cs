namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class AuthFeaturesWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerificationEnabled"] = "true";
    overrides["Auth__PasswordExpirationEnabled"] = "true";
    overrides["Auth__MaximumPasswordAgeDays"] = "90";
    overrides["Auth__PublicRegistrationEnabled"] = "true";
  }
}

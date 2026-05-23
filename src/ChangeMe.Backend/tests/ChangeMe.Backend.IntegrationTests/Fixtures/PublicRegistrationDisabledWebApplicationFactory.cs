namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class PublicRegistrationDisabledWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__PublicRegistrationEnabled"] = "false";
  }
}

using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class PublicRegistrationDisabledWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides[$"{AuthOptions.SectionName}__Registration__PublicEnabled"] = "false";
  }
}

using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class EmailChangeDisabledWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides[$"{AuthOptions.SectionName}__EmailChange__Enabled"] = "false";
  }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EmailChangeDisabledIntegrationTestCollection
  : ICollectionFixture<EmailChangeDisabledWebApplicationFactory>
{
  public const string Name = "EmailChangeDisabledIntegrationTests";
}

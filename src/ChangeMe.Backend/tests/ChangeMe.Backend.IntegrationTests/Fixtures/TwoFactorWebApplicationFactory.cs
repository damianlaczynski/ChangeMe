using ChangeMe.Backend.Infrastructure.Auth;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class TwoFactorWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides[$"{AuthOptions.SectionName}__EmailVerification__Enabled"] = "false";
    overrides[$"{AuthOptions.SectionName}__TwoFactor__Enabled"] = "true";
    overrides[$"{AuthOptions.SectionName}__TwoFactor__Required"] = "false";
    overrides[$"{AuthOptions.SectionName}__Registration__PublicEnabled"] = "true";
  }
}

public sealed class TwoFactorRequiredWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides[$"{AuthOptions.SectionName}__EmailVerification__Enabled"] = "false";
    overrides[$"{AuthOptions.SectionName}__TwoFactor__Enabled"] = "true";
    overrides[$"{AuthOptions.SectionName}__TwoFactor__Required"] = "true";
    overrides[$"{AuthOptions.SectionName}__Registration__PublicEnabled"] = "true";
  }
}

[CollectionDefinition(Name)]
public sealed class TwoFactorIntegrationTestCollection : ICollectionFixture<TwoFactorWebApplicationFactory>
{
  public const string Name = "TwoFactorIntegrationTests";
}

[CollectionDefinition(Name)]
public sealed class TwoFactorRequiredIntegrationTestCollection : ICollectionFixture<TwoFactorRequiredWebApplicationFactory>
{
  public const string Name = "TwoFactorRequiredIntegrationTests";
}

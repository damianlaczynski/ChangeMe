using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class TwoFactorWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerification__Enabled"] = "false";
    overrides["Auth__TwoFactor__Enabled"] = "true";
    overrides["Auth__TwoFactor__Required"] = "false";
    overrides["Auth__Registration__PublicEnabled"] = "true";
  }
}

public sealed class TwoFactorRequiredWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerification__Enabled"] = "false";
    overrides["Auth__TwoFactor__Enabled"] = "true";
    overrides["Auth__TwoFactor__Required"] = "true";
    overrides["Auth__Registration__PublicEnabled"] = "true";
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

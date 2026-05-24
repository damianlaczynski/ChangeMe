using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public sealed class TwoFactorWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerificationEnabled"] = "false";
    overrides["Auth__TwoFactorAuthenticationEnabled"] = "true";
    overrides["Auth__TwoFactorAuthenticationRequired"] = "false";
    overrides["Auth__PublicRegistrationEnabled"] = "true";
  }
}

public sealed class TwoFactorRequiredWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerificationEnabled"] = "false";
    overrides["Auth__TwoFactorAuthenticationEnabled"] = "true";
    overrides["Auth__TwoFactorAuthenticationRequired"] = "true";
    overrides["Auth__PublicRegistrationEnabled"] = "true";
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

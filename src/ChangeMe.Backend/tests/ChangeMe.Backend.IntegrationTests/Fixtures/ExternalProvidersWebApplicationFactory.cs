using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public class ExternalProvidersWebApplicationFactoryBase : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    ApplyCommonAuthOverrides(overrides);
    ApplyExternalProviderOverrides(overrides);
    ApplyFactorySpecificAuthOverrides(overrides);
  }

  protected virtual void ApplyFactorySpecificAuthOverrides(Dictionary<string, string?> overrides)
  {
  }

  protected virtual void ApplyExternalProviderOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__ExternalProvidersEnabled"] = "true";
    overrides["Auth__ExternalProviders__0__ProviderKey"] = FakeOidcExternalAuthService.ProviderKey;
    overrides["Auth__ExternalProviders__0__DisplayName"] = "Test";
    overrides["Auth__ExternalProviders__0__Authority"] = "https://login.test";
    overrides["Auth__ExternalProviders__0__ClientId"] = "test-client";
    overrides["Auth__ExternalProviders__0__ClientSecret"] = "test-secret";
  }

  protected static void ApplyCommonAuthOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerificationEnabled"] = "false";
    overrides["Auth__PublicRegistrationEnabled"] = "true";
    overrides["Auth__TwoFactorAuthenticationEnabled"] = "false";
    overrides["Auth__TwoFactorAuthenticationRequired"] = "false";
    overrides["Auth__TrustIdentityProviderMfa"] = "false";
  }

  protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);
    builder.ConfigureServices(services =>
    {
      services.RemoveAll<IOidcExternalAuthService>();
      services.AddSingleton<IOidcExternalAuthService, FakeOidcExternalAuthService>();
    });
  }
}

public sealed class ExternalProvidersWebApplicationFactory : ExternalProvidersWebApplicationFactoryBase;

public sealed class ExternalProvidersRestrictedDomainWebApplicationFactory
  : ExternalProvidersWebApplicationFactoryBase
{
  protected override void ApplyExternalProviderOverrides(Dictionary<string, string?> overrides)
  {
    base.ApplyExternalProviderOverrides(overrides);
    overrides["Auth__ExternalProviders__0__AllowedEmailDomains__0"] = "allowed.test";
  }
}

public class ExternalProvidersTwoFactorRequiredWebApplicationFactory
  : ExternalProvidersWebApplicationFactoryBase
{
  protected override void ApplyFactorySpecificAuthOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__TwoFactorAuthenticationEnabled"] = "true";
    overrides["Auth__TwoFactorAuthenticationRequired"] = "true";
  }
}

public sealed class ExternalProvidersTwoFactorWebApplicationFactory
  : ExternalProvidersWebApplicationFactoryBase
{
  protected override void ApplyFactorySpecificAuthOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__TwoFactorAuthenticationEnabled"] = "true";
    overrides["Auth__TwoFactorAuthenticationRequired"] = "false";
  }
}

public sealed class ExternalProvidersTwoFactorTrustMfaWebApplicationFactory
  : ExternalProvidersTwoFactorRequiredWebApplicationFactory
{
  protected override void ApplyFactorySpecificAuthOverrides(Dictionary<string, string?> overrides)
  {
    base.ApplyFactorySpecificAuthOverrides(overrides);
    overrides["Auth__TrustIdentityProviderMfa"] = "true";
  }
}

[CollectionDefinition(Name)]
public sealed class ExternalProvidersIntegrationTestCollection
  : ICollectionFixture<ExternalProvidersWebApplicationFactory>
{
  public const string Name = "ExternalProvidersIntegrationTests";
}

[CollectionDefinition(Name)]
public sealed class ExternalProvidersRestrictedDomainIntegrationTestCollection
  : ICollectionFixture<ExternalProvidersRestrictedDomainWebApplicationFactory>
{
  public const string Name = "ExternalProvidersRestrictedDomainIntegrationTests";
}

[CollectionDefinition(Name)]
public sealed class ExternalProvidersTwoFactorRequiredIntegrationTestCollection
  : ICollectionFixture<ExternalProvidersTwoFactorRequiredWebApplicationFactory>
{
  public const string Name = "ExternalProvidersTwoFactorRequiredIntegrationTests";
}

[CollectionDefinition(Name)]
public sealed class ExternalProvidersTwoFactorTrustMfaIntegrationTestCollection
  : ICollectionFixture<ExternalProvidersTwoFactorTrustMfaWebApplicationFactory>
{
  public const string Name = "ExternalProvidersTwoFactorTrustMfaIntegrationTests";
}

[CollectionDefinition(Name)]
public sealed class ExternalProvidersTwoFactorIntegrationTestCollection
  : ICollectionFixture<ExternalProvidersTwoFactorWebApplicationFactory>
{
  public const string Name = "ExternalProvidersTwoFactorIntegrationTests";
}

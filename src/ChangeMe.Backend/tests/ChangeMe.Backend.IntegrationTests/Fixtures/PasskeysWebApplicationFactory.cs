using ChangeMe.Backend.Infrastructure.Auth.Passkey;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChangeMe.Backend.IntegrationTests.Fixtures;

public class PasskeysWebApplicationFactory : BackendWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    overrides["Auth__EmailVerification__Enabled"] = "false";
    overrides["Auth__Registration__PublicEnabled"] = "true";
    overrides["Auth__Passkeys__PasskeysAuthenticationEnabled"] = "true";
    overrides["Auth__Passkeys__PasskeysAuthenticationRequired"] = "false";
    overrides["Auth__Passkeys__DiscoverablePasskeySignInOnLogin"] = "false";
    overrides["Auth__Passkeys__OfferPasskeyEnrollmentPrompt"] = "true";
    overrides["Auth__Passkeys__MaxFailedPasskeyAttempts"] = "5";
    overrides["Auth__TwoFactor__Enabled"] = "true";
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);

    builder.ConfigureServices(services =>
    {
      services.RemoveAll<IPasskeyFido2Service>();
      services.AddScoped<IPasskeyFido2Service, FakePasskeyFido2Service>();
    });
  }
}

public sealed class PasskeysRequiredWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__PasskeysAuthenticationRequired"] = "true";
  }
}

public sealed class PasskeysDiscoverableWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__DiscoverablePasskeySignInOnLogin"] = "true";
  }
}

public sealed class PasskeysPasskeyOnlyAllowedWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__AllowPasskeyOnlyAccounts"] = "true";
  }
}

public sealed class PasskeysSatisfiesTwoFactorWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__PasskeySatisfiesTwoFactor"] = "true";
    overrides["Auth__TwoFactor__Enabled"] = "true";
  }
}

public sealed class PasskeysEmailVerificationWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__EmailVerification__Enabled"] = "true";
  }
}

public sealed class PasskeysPasswordExpirationWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__PasswordExpiration__Enabled"] = "true";
    overrides["Auth__PasswordExpiration__MaximumPasswordAgeDays"] = "90";
  }
}

public sealed class PasskeysMaxOneWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__MaximumPasskeysPerUser"] = "1";
  }
}

public sealed class PasskeysTwoFactorAndRequiredWebApplicationFactory : PasskeysWebApplicationFactory
{
  protected override void ConfigureAuthEnvironmentOverrides(Dictionary<string, string?> overrides)
  {
    base.ConfigureAuthEnvironmentOverrides(overrides);
    overrides["Auth__Passkeys__PasskeysAuthenticationRequired"] = "true";
    overrides["Auth__TwoFactor__Enabled"] = "true";
    overrides["Auth__TwoFactor__Required"] = "true";
  }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysEmailVerificationIntegrationTestCollection
  : ICollectionFixture<PasskeysEmailVerificationWebApplicationFactory>
{
  public const string Name = "PasskeysEmailVerificationIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysPasswordExpirationIntegrationTestCollection
  : ICollectionFixture<PasskeysPasswordExpirationWebApplicationFactory>
{
  public const string Name = "PasskeysPasswordExpirationIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysMaxOneIntegrationTestCollection
  : ICollectionFixture<PasskeysMaxOneWebApplicationFactory>
{
  public const string Name = "PasskeysMaxOneIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysTwoFactorAndRequiredIntegrationTestCollection
  : ICollectionFixture<PasskeysTwoFactorAndRequiredWebApplicationFactory>
{
  public const string Name = "PasskeysTwoFactorAndRequiredIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysDiscoverableIntegrationTestCollection
  : ICollectionFixture<PasskeysDiscoverableWebApplicationFactory>
{
  public const string Name = "PasskeysDiscoverableIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysPasskeyOnlyIntegrationTestCollection
  : ICollectionFixture<PasskeysPasskeyOnlyAllowedWebApplicationFactory>
{
  public const string Name = "PasskeysPasskeyOnlyIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysSatisfiesTwoFactorIntegrationTestCollection
  : ICollectionFixture<PasskeysSatisfiesTwoFactorWebApplicationFactory>
{
  public const string Name = "PasskeysSatisfiesTwoFactorIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysIntegrationTestCollection : ICollectionFixture<PasskeysWebApplicationFactory>
{
  public const string Name = "PasskeysIntegrationTests";
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PasskeysRequiredIntegrationTestCollection : ICollectionFixture<PasskeysRequiredWebApplicationFactory>
{
  public const string Name = "PasskeysRequiredIntegrationTests";
}

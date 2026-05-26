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

using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(ExternalProvidersIntegrationTestCollection.Name)]
public sealed class ExternalSignInFailureEndpointTests(ExternalProvidersWebApplicationFactory factory)
{
  [Fact]
  public async Task PostExternalComplete_WhenCodeIsInvalid_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var response = await ExternalAuthTestHelper.CompleteSignInRawAsync(
      client,
      state,
      FakeOidcExternalAuthService.InvalidCode,
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}

[Collection(ExternalProvidersRestrictedDomainIntegrationTestCollection.Name)]
public sealed class ExternalSignInRestrictedDomainEndpointTests(
  ExternalProvidersRestrictedDomainWebApplicationFactory factory)
{
  [Fact]
  public async Task PostExternalComplete_WhenEmailDomainNotAllowed_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"blocked-{Guid.NewGuid():N}@other.test";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var response = await ExternalAuthTestHelper.CompleteSignInRawAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task PostExternalComplete_WhenEmailDomainAllowed_ShouldRegisterAndSignIn()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"allowed-{Guid.NewGuid():N}@allowed.test";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var result = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);

    Assert.NotNull(result.AuthSession);
    Assert.Equal(email, result.AuthSession!.Email);
  }
}

[Collection(ExternalProvidersTwoFactorTrustMfaIntegrationTestCollection.Name)]
public sealed class ExternalSignInTrustMfaEndpointTests(
  ExternalProvidersTwoFactorTrustMfaWebApplicationFactory factory)
{
  [Fact]
  public async Task PostExternalComplete_WhenIdpMfaTrusted_ShouldSignInWithoutSetupRequired()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var result = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      "mfa",
      cancellationToken);

    Assert.NotNull(result.AuthSession);
    Assert.False(result.AuthSession!.TwoFactorSetupRequired);
  }
}

[Collection(ExternalProvidersTwoFactorRequiredIntegrationTestCollection.Name)]
public sealed class ExternalSignInTwoFactorRequiredEndpointTests(
  ExternalProvidersTwoFactorRequiredWebApplicationFactory factory)
{
  [Fact]
  public async Task PostExternalComplete_WhenTwoFactorRequiredWithoutIdpMfa_ShouldReturnSetupRequiredSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-2fa-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var result = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);

    Assert.NotNull(result.AuthSession);
    Assert.True(result.AuthSession!.TwoFactorSetupRequired);
  }
}

[Collection(ExternalProvidersTwoFactorIntegrationTestCollection.Name)]
public sealed class ExternalSignInTwoFactorChallengeEndpointTests(
  ExternalProvidersTwoFactorWebApplicationFactory factory)
{
  [Fact]
  public async Task PostExternalComplete_WhenExistingLoginAndTwoFactorEnabled_ShouldReturnChallenge()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-2fa-challenge-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var firstState = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      firstState,
      $"email:{email}",
      cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var totpService = scope.ServiceProvider.GetRequiredService<ITotpService>();
      var protector = scope.ServiceProvider.GetRequiredService<ITwoFactorSecretProtector>();
      var user = await db.Users.SingleAsync(x => x.Email == email, cancellationToken);
      user.EnableTwoFactor(protector.Protect(totpService.GenerateSecret()), DateTime.UtcNow);
      await db.SaveChangesAsync(cancellationToken);
    }

    var providerSubject = await ExternalAuthTestHelper.GetProviderSubjectAsync(
      factory,
      (await GetUserIdAsync(factory, email, cancellationToken)),
      cancellationToken: cancellationToken);

    var secondState = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var result = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      secondState,
      $"subject:{providerSubject}",
      cancellationToken);

    Assert.Null(result.AuthSession);
    Assert.NotNull(result.TwoFactorChallenge);
  }

  private static async Task<Guid> GetUserIdAsync(
    ExternalProvidersWebApplicationFactoryBase factory,
    string email,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.SingleAsync(x => x.Email == email, cancellationToken);
    return user.Id;
  }
}

[Collection(ExternalProvidersIntegrationTestCollection.Name)]
public sealed class ExternalSignInAccountManagementEndpointTests(ExternalProvidersWebApplicationFactory factory)
{
  [Fact]
  public async Task PostSetPassword_WhenExternalOnlyAfterStepUp_ShouldSetPassword()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-setpw-{Guid.NewGuid():N}@example.com";
    const string newPassword = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var signIn = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", signIn.AuthSession!.Token);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.SingleAsync(x => x.Email == email, cancellationToken);

    var withoutStepUp = await client.PostAsJsonAsync("/api/auth/set-password", new
    {
      NewPassword = newPassword,
      CurrentPassword = (string?)null,
      VerificationCode = (string?)null
    }, cancellationToken);
    Assert.Equal(HttpStatusCode.BadRequest, withoutStepUp.StatusCode);

    await ExternalAuthTestHelper.CompleteStepUpAsync(client, factory, user.Id, cancellationToken);

    var setPasswordResponse = await client.PostAsJsonAsync("/api/auth/set-password", new
    {
      NewPassword = newPassword,
      CurrentPassword = (string?)null,
      VerificationCode = (string?)null
    }, cancellationToken);
    setPasswordResponse.EnsureSuccessStatusCode();

    var userAfter = await db.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, cancellationToken);
    Assert.True(userAfter.HasPasswordSet);
  }

  [Fact]
  public async Task PostExternalStepUp_WhenLinkedProviderMatches_ShouldRecordStepUp()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-stepup-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var signIn = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", signIn.AuthSession!.Token);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users
      .Include(x => x.ExternalLogins)
      .SingleAsync(x => x.Email == email, cancellationToken);
    Assert.Null(user.ExternalLogins.Single().LastStepUpAtUtc);

    await ExternalAuthTestHelper.CompleteStepUpAsync(client, factory, user.Id, cancellationToken);

    var loginAfter = await db.ExternalLogins
      .AsNoTracking()
      .SingleAsync(x => x.UserId == user.Id, cancellationToken);
    Assert.NotNull(loginAfter.LastStepUpAtUtc);
  }
}

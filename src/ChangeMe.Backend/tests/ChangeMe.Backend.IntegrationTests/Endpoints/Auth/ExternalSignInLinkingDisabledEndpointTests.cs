using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(ExternalProvidersLinkingDisabledIntegrationTestCollection.Name)]
public sealed class ExternalSignInLinkingDisabledEndpointTests(
  ExternalProvidersLinkingDisabledWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAuthSettings_ShouldExposeLinkingDisabled()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/api/auth/settings", cancellationToken);
    response.EnsureSuccessStatusCode();

    var settings = await IntegrationApiJson.ReadValueAsync<AuthSettingsDto>(response.Content, cancellationToken);
    Assert.NotNull(settings);
    Assert.True(settings.ExternalProvidersEnabled);
    Assert.False(settings.ExternalProviderLinkingEnabled);
  }

  [Fact]
  public async Task PostExternalBeginLink_WhenAuthenticated_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/begin",
      new { },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(
      ExternalAuthUtils.ExternalProviderLinkingDisabledMessage,
      body,
      StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetAccount_WhenAuthenticated_ShouldNotExposeLinkableProviders()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/auth/account", cancellationToken);
    response.EnsureSuccessStatusCode();

    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(response.Content, cancellationToken);
    Assert.NotNull(account);
    Assert.Empty(account!.LinkableProviders);
  }

  [Fact]
  public async Task PostExternalCompleteLink_WhenPendingExists_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var otherEmail = $"other-{Guid.NewGuid():N}@example.com";

    var (_, codeChallenge, state, nonce) = ExternalAuthPkceUtils.CreateAuthorizationParameters();
    var pendingResult = ExternalAuthPending.CreateLink(
      FakeOidcExternalAuthService.ProviderKey,
      state,
      nonce,
      codeChallenge,
      "test-verifier",
      user.UserId,
      DateTime.UtcNow.AddMinutes(10));
    Assert.True(pendingResult.IsSuccess);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      await db.ExternalAuthPending.AddAsync(pendingResult.Value, cancellationToken);
      await db.SaveChangesAsync(cancellationToken);
    }

    var response = await user.Client.PostAsJsonAsync(
      "/api/auth/external/complete",
      new { Code = $"email:{otherEmail}", State = state },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(
      ExternalAuthUtils.ExternalProviderLinkingDisabledMessage,
      body,
      StringComparison.OrdinalIgnoreCase);

    await using var verifyScope = factory.Services.CreateAsyncScope();
    var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Assert.Empty(await verifyDb.ExternalLogins
      .Where(x => x.UserId == user.UserId)
      .ToListAsync(cancellationToken));
  }
}

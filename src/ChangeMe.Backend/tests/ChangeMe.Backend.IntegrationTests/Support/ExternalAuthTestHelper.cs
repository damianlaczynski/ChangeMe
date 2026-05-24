using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class ExternalAuthTestHelper
{
  public static async Task<string> BeginSignInAndGetStateAsync(
    ExternalProvidersWebApplicationFactoryBase factory,
    HttpClient client,
    string providerKey = FakeOidcExternalAuthService.ProviderKey,
    CancellationToken cancellationToken = default)
  {
    var response = await client.PostAsJsonAsync(
      $"/api/auth/external/{providerKey}/begin",
      new { },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    var body = await IntegrationApiJson.ReadValueAsync<BeginExternalSignInResponseDto>(
      response.Content,
      cancellationToken);
    Assert.NotNull(body?.AuthorizationUrl);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = await db.ExternalAuthPending
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstAsync(cancellationToken);

    return pending.State;
  }

  public static async Task<HttpResponseMessage> CompleteSignInRawAsync(
    HttpClient client,
    string state,
    string code,
    CancellationToken cancellationToken = default) =>
    await client.PostAsJsonAsync(
      "/api/auth/external/complete",
      new { Code = code, State = state },
      cancellationToken);

  public static async Task<ExternalSignInResponseDto> CompleteSignInAsync(
    HttpClient client,
    string state,
    string code,
    CancellationToken cancellationToken = default)
  {
    var response = await CompleteSignInRawAsync(client, state, code, cancellationToken);
    response.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<ExternalSignInResponseDto>(
      response.Content,
      cancellationToken))!;
  }

  public static async Task<string> GetProviderSubjectAsync(
    ExternalProvidersWebApplicationFactoryBase factory,
    Guid userId,
    string providerKey = FakeOidcExternalAuthService.ProviderKey,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var login = await db.ExternalLogins
      .SingleAsync(x => x.UserId == userId && x.ProviderKey == providerKey, cancellationToken);
    return login.ProviderSubject;
  }

  public static async Task CompleteStepUpAsync(
    HttpClient client,
    ExternalProvidersWebApplicationFactoryBase factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    var providerSubject = await GetProviderSubjectAsync(factory, userId, cancellationToken: cancellationToken);

    var beginResponse = await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/step-up/begin",
      new { },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = await db.ExternalAuthPending
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstAsync(cancellationToken);

    var completeResponse = await client.PostAsJsonAsync(
      "/api/auth/external/complete",
      new { Code = $"subject:{providerSubject}", State = pending.State },
      cancellationToken);
    completeResponse.EnsureSuccessStatusCode();

    var result = await IntegrationApiJson.ReadValueAsync<ExternalSignInResponseDto>(
      completeResponse.Content,
      cancellationToken);
    Assert.True(result?.ExternalStepUpCompleted);
  }
}

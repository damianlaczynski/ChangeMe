using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class PasskeyTestHelper
{
  public static async Task<PasskeyTestUser> CreateUserWithPasskeyAsync(
    PasskeysWebApplicationFactory factory,
    string? email = null,
    string password = "StrongPass123!",
    string passkeyName = "Integration passkey",
    byte[]? credentialId = null,
    CancellationToken cancellationToken = default)
  {
    email ??= $"passkey-{Guid.NewGuid():N}@example.com";
    credentialId ??= Encoding.UTF8.GetBytes($"integration-passkey-{Guid.NewGuid():N}");

    using var registerClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await registerClient.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Passkey",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await registerClient.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var token = ExtractToken(await loginResponse.Content.ReadAsStringAsync(cancellationToken));
    var authenticatedClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });
    authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var passkey = await RegisterPasskeyAsync(
      authenticatedClient,
      password,
      passkeyName,
      credentialId,
      cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var userId = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .Users.AsNoTracking()
      .Where(x => x.Email == email)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    return new PasskeyTestUser(authenticatedClient, userId, email, password, passkey, credentialId);
  }

  public static async Task<MyAccountPasskeyDto> RegisterPasskeyAsync(
    HttpClient authenticatedClient,
    string currentPassword,
    string name,
    byte[] credentialId,
    CancellationToken cancellationToken = default)
  {
    var beginResponse = await authenticatedClient.PostAsJsonAsync(
      "/api/auth/passkeys/register/begin",
      new
      {
        unused = (object?)null,
        currentPassword,
        verificationCode = (string?)null
      },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);

    var completeResponse = await authenticatedClient.PostAsJsonAsync(
      "/api/auth/passkeys/register/complete",
      new
      {
        ceremonyId = begin!.CeremonyId,
        attestationResponse = PasskeyTestResponses.CreateAttestationResponse(credentialId),
        name,
        currentPassword,
        verificationCode = (string?)null
      },
      cancellationToken);
    if (!completeResponse.IsSuccessStatusCode)
    {
      var errorBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
      throw new InvalidOperationException(
        $"Passkey register complete failed with {(int)completeResponse.StatusCode}: {errorBody}");
    }

    return (await IntegrationApiJson.ReadValueAsync<MyAccountPasskeyDto>(
      completeResponse.Content,
      cancellationToken))!;
  }

  public static async Task<LoginResponseDto> SignInWithPasskeyAsync(
    HttpClient client,
    string email,
    byte[] credentialId,
    CancellationToken cancellationToken = default)
  {
    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);

    var completeResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/complete",
      new
      {
        ceremonyId = begin!.CeremonyId,
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(credentialId)
      },
      cancellationToken);
    completeResponse.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(
      completeResponse.Content,
      cancellationToken))!;
  }

  public static async Task<bool> CompletePasskeyStepUpAsync(
    HttpClient authenticatedClient,
    byte[] credentialId,
    CancellationToken cancellationToken = default)
  {
    var beginResponse = await authenticatedClient.PostAsJsonAsync(
      "/api/auth/passkeys/step-up/begin",
      new { unused = (object?)null },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);

    var completeResponse = await authenticatedClient.PostAsJsonAsync(
      "/api/auth/passkeys/step-up/complete",
      new
      {
        ceremonyId = begin!.CeremonyId,
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(credentialId)
      },
      cancellationToken);
    completeResponse.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<bool>(completeResponse.Content, cancellationToken))!;
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);

    if (document.RootElement.TryGetProperty("value", out var valueElement)
        && valueElement.TryGetProperty("authSession", out var authSession)
        && authSession.TryGetProperty("token", out var tokenElement))
    {
      return tokenElement.GetString() ?? throw new InvalidOperationException("Token value is null.");
    }

    throw new InvalidOperationException("Token was not found in login response.");
  }
}

internal sealed record PasskeyTestUser(
  HttpClient Client,
  Guid UserId,
  string Email,
  string Password,
  MyAccountPasskeyDto Passkey,
  byte[] CredentialId);

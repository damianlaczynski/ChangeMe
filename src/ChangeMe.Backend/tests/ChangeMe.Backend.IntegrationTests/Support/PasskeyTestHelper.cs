using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
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

    if (!loginResponse.IsSuccessStatusCode)
    {
      await using var verifyScope = factory.Services.CreateAsyncScope();
      var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var unverifiedUser = await verifyContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
      unverifiedUser.MarkEmailVerified();
      await verifyContext.SaveChangesAsync(cancellationToken);

      loginResponse = await registerClient.PostAsJsonAsync("/api/auth/login", new
      {
        Email = email,
        Password = password
      }, cancellationToken);
    }

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

  public static async Task<HttpResponseMessage> CompletePasskeySignInAsync(
    HttpClient client,
    string? email,
    byte[] credentialId,
    bool userVerification = true,
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

    return await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/complete",
      new
      {
        ceremonyId = begin!.CeremonyId,
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(credentialId, userVerification)
      },
      cancellationToken);
  }

  public static async Task<LoginResponseDto> SignInWithPasskeyAsync(
    HttpClient client,
    string email,
    byte[] credentialId,
    CancellationToken cancellationToken = default)
  {
    var completeResponse = await CompletePasskeySignInAsync(
      client,
      email,
      credentialId,
      userVerification: true,
      cancellationToken);

    completeResponse.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(
      completeResponse.Content,
      cancellationToken))!;
  }

  public static async Task AddExternalLoginAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    string providerKey = "google",
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var login = ExternalLogin.Create(userId, providerKey, $"subject-{Guid.NewGuid():N}").Value;
    await context.ExternalLogins.AddAsync(login, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task ClearLocalPasswordAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Users
      .Where(x => x.Id == userId)
      .ExecuteUpdateAsync(
        setters => setters
          .SetProperty(x => x.HasPasswordSet, false)
          .SetProperty(x => x.PasswordHash, string.Empty),
        cancellationToken);
  }

  public static async Task EnableTwoFactorAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.Users
      .Where(x => x.Id == userId)
      .ExecuteUpdateAsync(
        setters => setters.SetProperty(x => x.TwoFactorEnabled, true),
        cancellationToken);
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

  public static async Task<PasskeyCeremonyBeginResponseDto> BeginPasskeySignInAsync(
    HttpClient client,
    string? email,
    CancellationToken cancellationToken = default)
  {
    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken))!;
  }

  public static async Task<AuthResponseDto> RefreshSessionAsync(
    HttpClient client,
    string refreshToken,
    CancellationToken cancellationToken = default)
  {
    var response = await client.PostAsJsonAsync(
      "/api/auth/refresh",
      new { refreshToken },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    return (await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(
      response.Content,
      cancellationToken))!;
  }

  public static async Task DeactivateUserAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await context.Users.SingleAsync(x => x.Id == userId, cancellationToken);
    user.Deactivate();
    await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task SetInvitationPendingAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await context.Users
      .Include(x => x.AccountInvitations)
      .SingleAsync(x => x.Id == userId, cancellationToken);
    var utcNow = DateTime.UtcNow;
    user.RecordInvitationIssued(utcNow, utcNow.AddHours(72));
    await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task SetEmailVerifiedAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    bool emailVerified,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await context.Users.SingleAsync(x => x.Id == userId, cancellationToken);

    if (emailVerified)
      user.MarkEmailVerified();
    else
    {
      typeof(User).GetProperty(nameof(User.EmailVerified))!.SetValue(user, false);
      typeof(User).GetProperty(nameof(User.EmailVerifiedAt))!.SetValue(user, null);
    }

    await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task ExpirePasswordAsync(
    PasskeysWebApplicationFactory factory,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await context.Users.SingleAsync(x => x.Id == userId, cancellationToken);
    typeof(User).GetProperty(nameof(User.PasswordLastChangedAt))!
      .SetValue(user, DateTime.UtcNow.AddDays(-91));
    await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task ExpireCeremonyAsync(
    PasskeysWebApplicationFactory factory,
    Guid ceremonyId,
    CancellationToken cancellationToken = default)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await context.WebAuthnCeremonyPending
      .Where(x => x.Id == ceremonyId)
      .ExecuteUpdateAsync(
        setters => setters.SetProperty(x => x.ExpiresAtUtc, DateTime.UtcNow.AddMinutes(-1)),
        cancellationToken);
  }

  public static async Task<HttpResponseMessage> BeginPasskeyRegistrationAsync(
    HttpClient authenticatedClient,
    string? currentPassword,
    string? verificationCode = null,
    CancellationToken cancellationToken = default) =>
    await authenticatedClient.PostAsJsonAsync(
      "/api/auth/passkeys/register/begin",
      new
      {
        unused = (object?)null,
        currentPassword,
        verificationCode
      },
      cancellationToken);

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

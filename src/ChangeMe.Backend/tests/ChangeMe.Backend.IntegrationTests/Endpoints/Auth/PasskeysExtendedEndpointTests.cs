using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(PasskeysIntegrationTestCollection.Name)]
public sealed class PasskeysSignInGatesEndpointTests(PasskeysWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenAccountDeactivated_ShouldReturnDeactivatedMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.DeactivateUserAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var completeResponse = await PasskeyTestHelper.CompletePasskeySignInAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      cancellationToken: cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(AuthSessionUtils.DeactivatedAccountMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenInvitationPending_ShouldReturnInvitePendingMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.SetInvitationPendingAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var completeResponse = await PasskeyTestHelper.CompletePasskeySignInAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      cancellationToken: cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(AuthSessionUtils.InvitePendingAccountMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInBegin_WhenNoPasskeyRegistered_ShouldReturnNoPasskeyMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"no-passkey-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "No",
      LastName = "Passkey",
      Email = email,
      Password = password
    }, cancellationToken);

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, beginResponse.StatusCode);

    var responseBody = await beginResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.NoPasskeyForAccountMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenCeremonyExpired_ShouldReturnTimedOutMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var begin = await PasskeyTestHelper.BeginPasskeySignInAsync(client, testUser.Email, cancellationToken);
    await PasskeyTestHelper.ExpireCeremonyAsync(factory, begin.CeremonyId, cancellationToken);

    var completeResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/complete",
      new
      {
        ceremonyId = begin.CeremonyId,
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(testUser.CredentialId)
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.TimedOutMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenTwoFactorEnabled_ShouldReturnTwoFactorChallenge()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.EnableTwoFactorAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var login = await PasskeyTestHelper.SignInWithPasskeyAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      cancellationToken);

    Assert.Null(login.AuthSession);
    Assert.NotNull(login.TwoFactorChallenge);
  }
}

[Collection(PasskeysDiscoverableIntegrationTestCollection.Name)]
public sealed class PasskeysDiscoverableGatesEndpointTests(PasskeysDiscoverableWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenUnknownCredential_ShouldReturnNoMatchMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var begin = await PasskeyTestHelper.BeginPasskeySignInAsync(client, email: null, cancellationToken);

    var completeResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/complete",
      new
      {
        ceremonyId = begin.CeremonyId,
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(PasskeyTestResponses.AlternateCredentialId)
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.NoMatchMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysEmailVerificationIntegrationTestCollection.Name)]
public sealed class PasskeysEmailVerificationEndpointTests(PasskeysEmailVerificationWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenEmailNotVerified_ShouldReturnEmailNotVerifiedMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.SetEmailVerifiedAsync(factory, testUser.UserId, emailVerified: false, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var completeResponse = await PasskeyTestHelper.CompletePasskeySignInAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      cancellationToken: cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(AuthSessionUtils.EmailNotVerifiedMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysPasswordExpirationIntegrationTestCollection.Name)]
public sealed class PasskeysPasswordExpirationEndpointTests(PasskeysPasswordExpirationWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenPasswordExpired_ShouldReturnPasswordChangeRequired()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ExpirePasswordAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var login = await PasskeyTestHelper.SignInWithPasskeyAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      cancellationToken);

    Assert.NotNull(login.AuthSession);
    Assert.True(login.AuthSession!.PasswordChangeRequired);
    Assert.False(login.AuthSession.PasskeySetupRequired);
  }
}

[Collection(PasskeysTwoFactorAndRequiredIntegrationTestCollection.Name)]
public sealed class PasskeysComplianceGatesEndpointTests(PasskeysTwoFactorAndRequiredWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenTwoFactorAndPasskeysRequired_ShouldRequireTwoFactorSetupFirst()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"gates-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Gate",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);

    Assert.NotNull(login?.AuthSession);
    Assert.True(login.AuthSession!.TwoFactorSetupRequired);
    Assert.False(login.AuthSession.PasskeySetupRequired);
  }
}

[Collection(PasskeysRequiredIntegrationTestCollection.Name)]
public sealed class PasskeysStrictSetupEndpointTests(PasskeysRequiredWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeyRegisterBegin_WhenStrictSetupRequired_ShouldSucceedWithoutStepUp()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"strict-setup-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Strict",
      LastName = "Setup",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var beginResponse = await PasskeyTestHelper.BeginPasskeyRegistrationAsync(
      client,
      currentPassword: null,
      cancellationToken: cancellationToken);

    beginResponse.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task PostRefresh_WhenPasskeySetupRequired_ShouldReturnPasskeySetupRequired()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"refresh-passkey-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Refresh",
      LastName = "Passkey",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(login?.AuthSession?.RefreshToken);

    var refreshed = await PasskeyTestHelper.RefreshSessionAsync(
      client,
      login.AuthSession!.RefreshToken,
      cancellationToken);

    Assert.True(refreshed.PasskeySetupRequired);
  }
}

[Collection(PasskeysIntegrationTestCollection.Name)]
public sealed class PasskeysEnrollmentEndpointTests(PasskeysWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeyRename_WhenStepUpProvided_ShouldRenamePasskey()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var renameResponse = await testUser.Client.PostAsJsonAsync(
      $"/api/auth/passkeys/{testUser.Passkey.Id}/rename",
      new
      {
        passkeyId = testUser.Passkey.Id,
        name = "Renamed passkey",
        currentPassword = testUser.Password,
        verificationCode = (string?)null
      },
      cancellationToken);
    renameResponse.EnsureSuccessStatusCode();

    var renamed = await IntegrationApiJson.ReadValueAsync<MyAccountPasskeyDto>(
      renameResponse.Content,
      cancellationToken);

    Assert.Equal("Renamed passkey", renamed!.Name);
  }

  [Fact]
  public async Task PostPasskeyRename_WhenTwoFactorEnabledAndPasskeyStepUpOnly_ShouldRequireVerificationCode()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.EnableTwoFactorAsync(factory, testUser.UserId, cancellationToken);
    await PasskeyTestHelper.CompletePasskeyStepUpAsync(
      testUser.Client,
      testUser.CredentialId,
      cancellationToken);

    var renameResponse = await testUser.Client.PostAsJsonAsync(
      $"/api/auth/passkeys/{testUser.Passkey.Id}/rename",
      new
      {
        passkeyId = testUser.Passkey.Id,
        name = "Should fail",
        currentPassword = (string?)null,
        verificationCode = (string?)null
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, renameResponse.StatusCode);

    var responseBody = await renameResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Verification code is required.", responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysMaxOneIntegrationTestCollection.Name)]
public sealed class PasskeysMaxOneEnrollmentEndpointTests(PasskeysMaxOneWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeyRegisterBegin_WhenMaximumIsOneAndPasskeyExists_ShouldReturnMaximumMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var beginResponse = await PasskeyTestHelper.BeginPasskeyRegistrationAsync(
      testUser.Client,
      testUser.Password,
      cancellationToken: cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, beginResponse.StatusCode);

    var responseBody = await beginResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.MaximumPasskeysMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysPasskeyOnlyIntegrationTestCollection.Name)]
public sealed class PasskeysAdminRemoveEndpointTests(PasskeysPasskeyOnlyAllowedWebApplicationFactory factory)
{
  [Fact]
  public async Task PostRemoveUserPasskey_WhenOnlySignInMethod_ShouldReturnError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ClearLocalPasswordAsync(factory, testUser.UserId, cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{testUser.UserId}/passkeys/{testUser.Passkey.Id}/remove",
      new
      {
        id = testUser.UserId,
        passkeyId = testUser.Passkey.Id
      },
      cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.RemoveOnlySignInMethodMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysIntegrationTestCollection.Name)]
public sealed class PasskeysAdminSessionEndpointTests(PasskeysWebApplicationFactory factory)
{
  [Fact]
  public async Task PostRemoveUserPasskey_WhenPasswordRemains_ShouldKeepActiveSessions()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{testUser.UserId}/passkeys/{testUser.Passkey.Id}/remove",
      new
      {
        id = testUser.UserId,
        passkeyId = testUser.Passkey.Id
      },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var activeSessions = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .UserSessions
      .CountAsync(x => x.UserId == testUser.UserId && x.RevokedAt == null, cancellationToken);

    Assert.True(activeSessions >= 1);
  }
}

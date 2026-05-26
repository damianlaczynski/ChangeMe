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

[Collection(IntegrationTestCollection.Name)]
public sealed class PasskeysDisabledEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInBegin_WhenPasskeysDisabled_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = "any@example.com" },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }
}

[Collection(PasskeysIntegrationTestCollection.Name)]
public sealed class PasskeysEndpointTests(PasskeysWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAuthSettings_WhenPasskeysEnabled_ShouldExposePasskeyFlags()
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
    Assert.True(settings.Passkeys.PasskeysAuthenticationEnabled);
    Assert.False(settings.Passkeys.PasskeysAuthenticationRequired);
    Assert.False(settings.Passkeys.DiscoverablePasskeySignInOnLogin);
    Assert.True(settings.Passkeys.OfferPasskeyEnrollmentPrompt);
  }

  [Fact]
  public async Task PostPasskeyRegisterComplete_ShouldPersistPasskey()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var passkeyCount = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .PasskeyCredentials
      .CountAsync(x => x.UserId == testUser.UserId, cancellationToken);

    Assert.Equal(1, passkeyCount);
    Assert.Equal("Integration passkey", testUser.Passkey.Name);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_ShouldReturnAuthSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

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
    Assert.False(string.IsNullOrWhiteSpace(login.AuthSession!.Token));
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenCeremonyEmailDoesNotMatchCredential_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var userA = await PasskeyTestHelper.CreateUserWithPasskeyAsync(
      factory,
      email: $"passkey-a-{Guid.NewGuid():N}@example.com",
      cancellationToken: cancellationToken);
    var userB = await PasskeyTestHelper.CreateUserWithPasskeyAsync(
      factory,
      email: $"passkey-b-{Guid.NewGuid():N}@example.com",
      credentialId: PasskeyTestResponses.AlternateCredentialId,
      cancellationToken: cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = userA.Email },
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
        assertionResponse = PasskeyTestResponses.CreateAssertionResponse(userB.CredentialId)
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.NoMatchMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeyStepUpComplete_ShouldRecordStepUpTimestamp()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var completed = await PasskeyTestHelper.CompletePasskeyStepUpAsync(
      testUser.Client,
      testUser.CredentialId,
      cancellationToken);

    Assert.True(completed);

    await using var scope = factory.Services.CreateAsyncScope();
    var stepUpCompletedAt = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .Users
      .AsNoTracking()
      .Where(x => x.Id == testUser.UserId)
      .Select(x => x.PasskeyStepUpCompletedAt)
      .SingleAsync(cancellationToken);

    Assert.NotNull(stepUpCompletedAt);
  }

  [Fact]
  public async Task PostPasskeyStepUpComplete_WhenTooManyFailedAttemptsOnSameCeremony_ShouldReturnTooManyAttempts()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var beginResponse = await testUser.Client.PostAsJsonAsync(
      "/api/auth/passkeys/step-up/begin",
      new { unused = (object?)null },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);

    HttpResponseMessage? lastResponse = null;
    for (var attempt = 0; attempt < 5; attempt++)
    {
      lastResponse = await testUser.Client.PostAsJsonAsync(
        "/api/auth/passkeys/step-up/complete",
        new
        {
          ceremonyId = begin!.CeremonyId,
          assertionResponse = PasskeyTestResponses.CreateAssertionResponse(PasskeyTestResponses.AlternateCredentialId)
        },
        cancellationToken);
    }

    Assert.NotNull(lastResponse);
    Assert.Equal(HttpStatusCode.Unauthorized, lastResponse.StatusCode);

    var responseBody = await lastResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.TooManyAttemptsMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeyRemove_WhenPasskeysOptionalAndPasswordSet_ShouldRemovePasskey()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var removeResponse = await testUser.Client.PostAsJsonAsync(
      $"/api/auth/passkeys/{testUser.Passkey.Id}/remove",
      new
      {
        currentPassword = testUser.Password,
        verificationCode = (string?)null
      },
      cancellationToken);
    removeResponse.EnsureSuccessStatusCode();

    var removed = await IntegrationApiJson.ReadValueAsync<bool>(removeResponse.Content, cancellationToken);
    Assert.True(removed);

    await using var scope = factory.Services.CreateAsyncScope();
    var passkeyCount = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .PasskeyCredentials
      .CountAsync(x => x.UserId == testUser.UserId, cancellationToken);

    Assert.Equal(0, passkeyCount);
  }

  [Fact]
  public async Task PostResetUserPasskeys_WhenAdministrator_ShouldRemoveAllPasskeys()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{testUser.UserId}/reset-passkeys",
      new { id = testUser.UserId },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var passkeyCount = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .PasskeyCredentials
      .CountAsync(x => x.UserId == testUser.UserId, cancellationToken);

    Assert.Equal(0, passkeyCount);
  }

  [Fact]
  public async Task PostRemoveUserPasskey_WhenAdministrator_ShouldRemoveSinglePasskey()
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
    var passkeyCount = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .PasskeyCredentials
      .CountAsync(x => x.UserId == testUser.UserId, cancellationToken);

    Assert.Equal(0, passkeyCount);
  }

  [Fact]
  public async Task PostPasskeyRegisterBegin_WhenPasskeysOptionalAndStepUpMissing_ShouldReturnValidationError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"passkey-no-stepup-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "No",
      LastName = "StepUp",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var token = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(
      loginResponse.Content,
      cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AuthSession!.Token);

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/register/begin",
      new
      {
        unused = (object?)null,
        currentPassword = (string?)null,
        verificationCode = (string?)null
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, beginResponse.StatusCode);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenTooManyFailedAttemptsOnSameCeremony_ShouldReturnTooManyAttempts()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = testUser.Email },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);

    HttpResponseMessage? lastResponse = null;
    for (var attempt = 0; attempt < 5; attempt++)
    {
      lastResponse = await client.PostAsJsonAsync(
        "/api/auth/passkeys/sign-in/complete",
        new
        {
          ceremonyId = begin!.CeremonyId,
          assertionResponse = PasskeyTestResponses.CreateAssertionResponse(PasskeyTestResponses.AlternateCredentialId)
        },
        cancellationToken);
    }

    Assert.NotNull(lastResponse);
    Assert.Equal(HttpStatusCode.Unauthorized, lastResponse.StatusCode);

    var responseBody = await lastResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.TooManyAttemptsMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInBegin_WhenPasskeyOnlyNotAllowed_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ClearLocalPasswordAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = testUser.Email },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, beginResponse.StatusCode);

    var responseBody = await beginResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.PasskeyOnlyNotAllowedMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInBegin_WhenExternalOnlyWithPasskey_ShouldBeginCeremony()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ClearLocalPasswordAsync(factory, testUser.UserId, cancellationToken);
    await PasskeyTestHelper.AddExternalLoginAsync(factory, testUser.UserId, cancellationToken: cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = testUser.Email },
      cancellationToken);

    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      beginResponse.Content,
      cancellationToken);
    Assert.NotNull(begin?.CeremonyId);
    Assert.NotNull(begin.Options);
  }

  [Fact]
  public async Task PostResetUserPasskeys_WhenAdministrator_ShouldRevokeActiveSessions()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var activeBefore = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
        .UserSessions
        .CountAsync(x => x.UserId == testUser.UserId && x.RevokedAt == null, cancellationToken);

      Assert.True(activeBefore >= 1);
    }

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{testUser.UserId}/reset-passkeys",
      new { id = testUser.UserId },
      cancellationToken);
    response.EnsureSuccessStatusCode();

    await using var verifyScope = factory.Services.CreateAsyncScope();
    var activeAfter = await verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
      .UserSessions
      .CountAsync(x => x.UserId == testUser.UserId && x.RevokedAt == null, cancellationToken);

    Assert.Equal(0, activeAfter);
  }

  [Fact]
  public async Task PostPasskeyRemove_WhenOnlySignInMethod_ShouldReturnError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ClearLocalPasswordAsync(factory, testUser.UserId, cancellationToken);

    var stepUpCompleted = await PasskeyTestHelper.CompletePasskeyStepUpAsync(
      testUser.Client,
      testUser.CredentialId,
      cancellationToken);
    Assert.True(stepUpCompleted);

    var removeResponse = await testUser.Client.PostAsJsonAsync(
      $"/api/auth/passkeys/{testUser.Passkey.Id}/remove",
      new
      {
        currentPassword = (string?)null,
        verificationCode = (string?)null
      },
      cancellationToken);

    var responseBody = await removeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.RemoveOnlySignInMethodMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

[Collection(PasskeysDiscoverableIntegrationTestCollection.Name)]
public sealed class PasskeysDiscoverableEndpointTests(PasskeysDiscoverableWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInBegin_WhenDiscoverableEnabled_ShouldSucceedWithoutEmail()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync(
      "/api/auth/passkeys/sign-in/begin",
      new { email = (string?)null },
      cancellationToken);

    response.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<PasskeyCeremonyBeginResponseDto>(
      response.Content,
      cancellationToken);

    Assert.NotNull(begin?.CeremonyId);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenDiscoverableEnabled_ShouldReturnAuthSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var completeResponse = await PasskeyTestHelper.CompletePasskeySignInAsync(
      client,
      email: null,
      testUser.CredentialId,
      cancellationToken: cancellationToken);

    completeResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(
      completeResponse.Content,
      cancellationToken);

    Assert.NotNull(login?.AuthSession);
    Assert.False(string.IsNullOrWhiteSpace(login.AuthSession!.Token));
  }
}

[Collection(PasskeysPasskeyOnlyIntegrationTestCollection.Name)]
public sealed class PasskeysPasskeyOnlyEndpointTests(PasskeysPasskeyOnlyAllowedWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenPasskeyOnlyAllowed_ShouldReturnAuthSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.ClearLocalPasswordAsync(factory, testUser.UserId, cancellationToken);

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
    Assert.False(string.IsNullOrWhiteSpace(login.AuthSession!.Token));
  }
}

[Collection(PasskeysSatisfiesTwoFactorIntegrationTestCollection.Name)]
public sealed class PasskeysSatisfiesTwoFactorEndpointTests(PasskeysSatisfiesTwoFactorWebApplicationFactory factory)
{
  [Fact]
  public async Task PostPasskeySignInComplete_WhenUserVerificationMissing_ShouldReturnUvRequiredMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);
    await PasskeyTestHelper.EnableTwoFactorAsync(factory, testUser.UserId, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var completeResponse = await PasskeyTestHelper.CompletePasskeySignInAsync(
      client,
      testUser.Email,
      testUser.CredentialId,
      userVerification: false,
      cancellationToken: cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, completeResponse.StatusCode);

    var responseBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.UvRequiredMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeySignInComplete_WhenUserVerificationPresent_ShouldSkipTwoFactorChallenge()
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

    Assert.NotNull(login.AuthSession);
    Assert.Null(login.TwoFactorChallenge);
  }
}

[Collection(PasskeysRequiredIntegrationTestCollection.Name)]
public sealed class PasskeysRequiredEndpointTests(PasskeysRequiredWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenPasskeysRequiredAndNotEnrolled_ShouldReturnSetupRequiredSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"passkey-required-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Required",
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

    Assert.NotNull(login?.AuthSession);
    Assert.True(login.AuthSession!.PasskeySetupRequired);
  }

  [Fact]
  public async Task GetIssues_WhenPasskeySetupRequired_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"passkey-blocked-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Blocked",
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
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var issuesResponse = await client.GetAsync("/api/issues?pageNumber=1&pageSize=10", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, issuesResponse.StatusCode);

    var responseBody = await issuesResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Passkey setup is required to continue.", responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostPasskeyRemove_WhenOnlyPasskeyAndPasskeysRequired_ShouldReturnError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var testUser = await PasskeyTestHelper.CreateUserWithPasskeyAsync(factory, cancellationToken: cancellationToken);

    var removeResponse = await testUser.Client.PostAsJsonAsync(
      $"/api/auth/passkeys/{testUser.Passkey.Id}/remove",
      new
      {
        currentPassword = testUser.Password,
        verificationCode = (string?)null
      },
      cancellationToken);

    var responseBody = await removeResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PasskeyAuthUtils.RemoveRequiredPasskeyMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

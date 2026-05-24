using System.Net;
using System.Net.Http.Json;
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
public sealed class ExternalSignInEndpointTests(ExternalProvidersWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAuthSettings_WhenExternalProvidersEnabled_ShouldExposeConfiguredProvider()
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
    Assert.Single(settings.ExternalProviders);
    Assert.Equal(FakeOidcExternalAuthService.ProviderKey, settings.ExternalProviders[0].ProviderKey);
  }

  [Fact]
  public async Task PostExternalBegin_WhenAnonymous_ShouldReturnAuthorizationUrl()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/begin",
      new { },
      cancellationToken);

    response.EnsureSuccessStatusCode();
    var body = await IntegrationApiJson.ReadValueAsync<BeginExternalSignInResponseDto>(
      response.Content,
      cancellationToken);
    Assert.False(string.IsNullOrWhiteSpace(body?.AuthorizationUrl));
  }

  [Fact]
  public async Task PostExternalComplete_WhenNewVerifiedEmail_ShouldRegisterAndSignIn()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-new-{Guid.NewGuid():N}@example.com";

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

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users
      .Include(x => x.ExternalLogins)
      .SingleAsync(x => x.Email == email, cancellationToken);
    Assert.Single(user.ExternalLogins);
    Assert.False(user.HasPasswordSet);
  }

  [Fact]
  public async Task PostExternalComplete_WhenEmailMatchesPasswordAccount_ShouldRequireLink()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-link-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Local",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var result = await ExternalAuthTestHelper.CompleteSignInAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);

    Assert.Null(result.AuthSession);
    Assert.NotNull(result.LinkAccountRequired);
    Assert.Equal(email, result.LinkAccountRequired!.Email);

    var linkResponse = await client.PostAsJsonAsync("/api/auth/external/link", new
    {
      State = result.LinkAccountRequired.State,
      Password = password
    }, cancellationToken);
    linkResponse.EnsureSuccessStatusCode();

    var linked = await IntegrationApiJson.ReadValueAsync<ExternalSignInResponseDto>(
      linkResponse.Content,
      cancellationToken);
    Assert.NotNull(linked?.AuthSession);
  }

  [Fact]
  public async Task PostExternalLinkAndUnlink_WhenAuthenticated_ShouldManageExternalLogins()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-account-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Account",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();
    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var beginLinkResponse = await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/begin",
      new { },
      cancellationToken);
    beginLinkResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = await db.ExternalAuthPending
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstAsync(cancellationToken);

    var completeLinkResponse = await client.PostAsJsonAsync("/api/auth/external/complete", new
    {
      Code = $"email:{email}",
      State = pending.State
    }, cancellationToken);
    completeLinkResponse.EnsureSuccessStatusCode();
    var linked = await IntegrationApiJson.ReadValueAsync<ExternalSignInResponseDto>(
      completeLinkResponse.Content,
      cancellationToken);
    Assert.True(linked?.AccountLinkCompleted);

    var accountResponse = await client.GetAsync("/api/auth/account", cancellationToken);
    accountResponse.EnsureSuccessStatusCode();
    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(accountResponse.Content, cancellationToken);
    Assert.Single(account!.ExternalLogins);

    var unlinkResponse = await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/unlink",
      new { CurrentPassword = password, VerificationCode = (string?)null },
      cancellationToken);
    unlinkResponse.EnsureSuccessStatusCode();

    var accountAfterUnlink = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(
      (await client.GetAsync("/api/auth/account", cancellationToken)).Content,
      cancellationToken);
    Assert.Empty(accountAfterUnlink!.ExternalLogins);
  }

  [Fact]
  public async Task PostExternalCompleteLink_WhenProviderEmailDoesNotMatchAccount_ShouldFail()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-link-mismatch-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Link",
      LastName = "Mismatch",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();
    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/begin",
      new { },
      cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = await db.ExternalAuthPending
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstAsync(cancellationToken);

    var completeLinkResponse = await client.PostAsJsonAsync("/api/auth/external/complete", new
    {
      Code = $"email:other-{Guid.NewGuid():N}@example.com",
      State = pending.State
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, completeLinkResponse.StatusCode);

    var accountResponse = await client.GetAsync("/api/auth/account", cancellationToken);
    accountResponse.EnsureSuccessStatusCode();
    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(accountResponse.Content, cancellationToken);
    Assert.Empty(account!.ExternalLogins);
  }

  [Fact]
  public async Task PostExternalComplete_WhenLinkedProviderSubjectDoesNotMatch_ShouldNotSignInByEmailOnly()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-subject-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Subject",
      LastName = "Guard",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();
    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    await client.PostAsJsonAsync(
      $"/api/auth/external/{FakeOidcExternalAuthService.ProviderKey}/begin",
      new { },
      cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = await db.ExternalAuthPending
      .OrderByDescending(x => x.ExpiresAtUtc)
      .FirstAsync(cancellationToken);

    var completeLinkResponse = await client.PostAsJsonAsync("/api/auth/external/complete", new
    {
      Code = $"email:{email}",
      State = pending.State
    }, cancellationToken);
    completeLinkResponse.EnsureSuccessStatusCode();

    client.DefaultRequestHeaders.Authorization = null;

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    var signInResponse = await ExternalAuthTestHelper.CompleteSignInRawAsync(
      client,
      state,
      $"email:{email}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, signInResponse.StatusCode);
  }

  [Fact]
  public async Task PostAdminUnlink_WhenLastSignInMethod_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-only-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var state = await ExternalAuthTestHelper.BeginSignInAndGetStateAsync(
      factory,
      client,
      cancellationToken: cancellationToken);
    await ExternalAuthTestHelper.CompleteSignInAsync(client, state, $"email:{email}", cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.SingleAsync(x => x.Email == email, cancellationToken);

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var unlinkResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.Id}/external-logins/{FakeOidcExternalAuthService.ProviderKey}/unlink",
      new { },
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, unlinkResponse.StatusCode);
  }

  [Fact]
  public async Task PostExternalComplete_WhenInvitationPendingAndVerifiedEmail_ShouldAcceptInvitationAndSignIn()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-invite-{Guid.NewGuid():N}@example.com";

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await GetRoleIdByNameAsync(factory, "User", cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/users", new
    {
      FirstName = "",
      LastName = "",
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

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

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users
      .Include(x => x.ExternalLogins)
      .SingleAsync(x => x.Email == email, cancellationToken);

    Assert.False(user.HasPasswordSet);
    Assert.Null(user.InvitationSentAt);
    Assert.Single(user.ExternalLogins);
    Assert.Equal("Oidc", user.FirstName);
    Assert.Equal("User", user.LastName);

    var fakeEmail = scope.ServiceProvider.GetRequiredService<ChangeMe.Backend.Domain.Common.IEmailService>()
      as FakeEmailService;
    Assert.NotNull(fakeEmail);
    Assert.Contains(fakeEmail.SentEmails, e => e.Subject == "External sign-in method linked");
  }

  [Fact]
  public async Task PostExternalComplete_WhenInvitationPendingAndUnverifiedEmail_ShouldReject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"oidc-invite-unverified-{Guid.NewGuid():N}@example.com";

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await GetRoleIdByNameAsync(factory, "User", cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/users", new
    {
      FirstName = "Invited",
      LastName = "User",
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);
    createResponse.EnsureSuccessStatusCode();

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
      $"unverified:{email}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.NotNull(user.InvitationSentAt);
    Assert.Empty(await db.ExternalLogins.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken));
  }

  private static async Task<Guid> GetRoleIdByNameAsync(
    BackendWebApplicationFactory factory,
    string roleName,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Roles
      .AsNoTracking()
      .Where(x => x.Name == roleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);
  }
}

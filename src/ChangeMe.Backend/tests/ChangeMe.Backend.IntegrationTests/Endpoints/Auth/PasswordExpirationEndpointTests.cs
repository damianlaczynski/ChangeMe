using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(AuthFeaturesIntegrationTestCollection.Name)]
public sealed class PasswordExpirationEndpointTests(AuthFeaturesWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenPasswordExpired_ShouldReturnPasswordChangeRequired()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"expired-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Expired",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    user.MarkEmailVerified();
    typeof(ChangeMe.Backend.Domain.Aggregates.Users.User)
      .GetProperty("PasswordLastChangedAt")!
      .SetValue(user, DateTime.UtcNow.AddDays(-91));
    await dbContext.SaveChangesAsync(cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(login?.AuthSession);
    Assert.True(login.AuthSession!.PasswordChangeRequired);
    Assert.NotNull(login.AuthSession.PasswordExpiresAtUtc);
  }

  [Fact]
  public async Task PostLogin_WhenPasswordWithinAge_ShouldReturnPasswordExpiresAtUtc()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"valid-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Valid",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    user.MarkEmailVerified();
    await dbContext.SaveChangesAsync(cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(login?.AuthSession);
    Assert.False(login.AuthSession!.PasswordChangeRequired);
    Assert.NotNull(login.AuthSession.PasswordExpiresAtUtc);
    Assert.True(login.AuthSession.PasswordExpiresAtUtc > DateTime.UtcNow);
  }

  [Fact]
  public async Task GetIssues_WhenPasswordChangeRequired_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"expired-blocked-{Guid.NewGuid():N}@example.com";
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

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    user.MarkEmailVerified();
    typeof(ChangeMe.Backend.Domain.Aggregates.Users.User)
      .GetProperty("PasswordLastChangedAt")!
      .SetValue(user, DateTime.UtcNow.AddDays(-91));
    await dbContext.SaveChangesAsync(cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var issuesResponse = await client.GetAsync("/api/issues?pageNumber=1&pageSize=10", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, issuesResponse.StatusCode);

    var responseBody = await issuesResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(
      "Your password has expired. Set a new password to continue.",
      responseBody,
      StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetAuthSettings_WhenPasswordChangeRequired_ShouldAllowSettingsForSetupFlow()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"expired-settings-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Settings",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    user.MarkEmailVerified();
    typeof(ChangeMe.Backend.Domain.Aggregates.Users.User)
      .GetProperty("PasswordLastChangedAt")!
      .SetValue(user, DateTime.UtcNow.AddDays(-91));
    await dbContext.SaveChangesAsync(cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var settingsResponse = await client.GetAsync("/api/auth/settings", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, settingsResponse.StatusCode);
  }

  [Fact]
  public async Task RequiredChangePassword_WhenAuthenticatedWithExpiredPassword_ShouldAllowIssuesAccess()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"expired-change-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";
    const string newPassword = "NewStrongPass456!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Expired",
      LastName = "Change",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    user.MarkEmailVerified();
    typeof(ChangeMe.Backend.Domain.Aggregates.Users.User)
      .GetProperty("PasswordLastChangedAt")!
      .SetValue(user, DateTime.UtcNow.AddDays(-91));
    await dbContext.SaveChangesAsync(cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();
    var loginBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
    var token = ExtractToken(loginBody);

    var authenticatedClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });
    authenticatedClient.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var blockedResponse = await authenticatedClient.GetAsync("/api/issues?pageNumber=1&pageSize=10", cancellationToken);
    Assert.Equal(HttpStatusCode.Forbidden, blockedResponse.StatusCode);

    var changeResponse = await authenticatedClient.PostAsJsonAsync(
      "/api/auth/required-change-password",
      new { NewPassword = newPassword },
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

    var loginAfterChangeResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = newPassword,
    }, cancellationToken);

    var loginAfterChange = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(
      loginAfterChangeResponse.Content,
      cancellationToken);
    Assert.NotNull(loginAfterChange?.AuthSession);
    Assert.False(loginAfterChange.AuthSession!.PasswordChangeRequired);
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    var value = document.RootElement.GetProperty("value");
    if (value.TryGetProperty("authSession", out var authSession))
      return authSession.GetProperty("token").GetString()
        ?? throw new InvalidOperationException("Token was not found.");

    return value.GetProperty("token").GetString()
      ?? throw new InvalidOperationException("Token was not found.");
  }
}

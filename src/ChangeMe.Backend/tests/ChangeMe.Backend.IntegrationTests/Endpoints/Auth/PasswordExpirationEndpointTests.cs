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
      RememberMe = false
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var auth = await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(auth);
    Assert.True(auth!.PasswordChangeRequired);
    Assert.NotNull(auth.PasswordExpiresAtUtc);
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
      RememberMe = false
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();

    var auth = await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(auth);
    Assert.False(auth!.PasswordChangeRequired);
    Assert.NotNull(auth.PasswordExpiresAtUtc);
    Assert.True(auth.PasswordExpiresAtUtc > DateTime.UtcNow);
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
      RememberMe = false
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

    var loginAfterChange = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = newPassword,
      RememberMe = false
    }, cancellationToken);

    var auth = await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(loginAfterChange.Content, cancellationToken);
    Assert.NotNull(auth);
    Assert.False(auth!.PasswordChangeRequired);
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    return document.RootElement.GetProperty("value").GetProperty("token").GetString()
      ?? throw new InvalidOperationException("Token was not found.");
  }
}

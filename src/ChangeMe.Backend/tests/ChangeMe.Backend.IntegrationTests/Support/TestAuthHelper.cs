using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class TestAuthHelper
{
  public const string SeededAdminEmail = "integration-admin@example.com";
  public const string SeededAdminPassword = "IntegrationAdmin123!";
  public const string DefaultUserPassword = "StrongPass123!";

  public static async Task<HttpClient> CreateAuthenticatedClientAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var user = await CreateAuthenticatedUserAsync(factory, cancellationToken);
    return user.Client;
  }

  public static async Task<AuthenticatedTestUser> CreateAuthenticatedUserAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var admin = await CreateAdministratorUserAsync(factory, cancellationToken);
    var email = $"user-{Guid.NewGuid():N}@example.com";
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);

    var createResponse = await admin.Client.PostAsJsonAsync("/api/users", new
    {
      FirstName = "Test",
      LastName = "User",
      Email = email,
      Password = DefaultUserPassword,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();

    return await LoginAsUserAsync(factory, email, DefaultUserPassword, cancellationToken);
  }

  public static async Task<AuthenticatedTestUser> CreateAdministratorUserAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    return await LoginAsUserAsync(
      factory,
      SeededAdminEmail,
      SeededAdminPassword,
      cancellationToken);
  }

  private static async Task<AuthenticatedTestUser> LoginAsUserAsync(
    BackendWebApplicationFactory factory,
    string email,
    string password,
    CancellationToken cancellationToken)
  {
    using var loginClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();

    var responseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
    var token = ExtractToken(responseBody);

    var authenticatedClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userId = await dbContext.Users
      .AsNoTracking()
      .Where(u => u.Email == email)
      .Select(u => u.Id)
      .SingleAsync(cancellationToken);

    return new AuthenticatedTestUser(authenticatedClient, userId, email);
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);

    if (document.RootElement.TryGetProperty("value", out var valueElement))
    {
      if (valueElement.TryGetProperty("authSession", out var authSession) &&
          authSession.ValueKind == JsonValueKind.Object &&
          authSession.TryGetProperty("token", out var nestedToken))
      {
        return nestedToken.GetString() ?? throw new InvalidOperationException("Token value is null.");
      }

      if (valueElement.TryGetProperty("token", out var tokenElement))
      {
        return tokenElement.GetString() ?? throw new InvalidOperationException("Token value is null.");
      }
    }

    throw new InvalidOperationException("Token was not found in login response.");
  }
}

internal sealed record AuthenticatedTestUser(HttpClient Client, Guid UserId, string Email);

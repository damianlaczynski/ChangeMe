using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class TestAuthHelper
{
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
    var email = $"user-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var anonymousClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await anonymousClient.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Test",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await anonymousClient.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    if (!loginResponse.IsSuccessStatusCode)
    {
      await using var verifyScope = factory.Services.CreateAsyncScope();
      var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var unverifiedUser = await verifyDbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
      unverifiedUser.MarkEmailVerified();
      await verifyDbContext.SaveChangesAsync(cancellationToken);

      loginResponse = await anonymousClient.PostAsJsonAsync("/api/auth/login", new
      {
        Email = email,
        Password = password
      }, cancellationToken);
    }

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

  public static async Task<AuthenticatedTestUser> CreateAdministratorUserAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken = default)
  {
    var user = await CreateAuthenticatedUserAsync(factory, cancellationToken);

    await using (var scope = factory.Services.CreateAsyncScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var administratorRoleId = await dbContext.Roles
        .AsNoTracking()
        .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
        .Select(x => x.Id)
        .SingleAsync(cancellationToken);

      var alreadyAssigned = await dbContext.Set<UserRole>()
        .AnyAsync(x => x.UserId == user.UserId && x.RoleId == administratorRoleId, cancellationToken);

      if (!alreadyAssigned)
      {
        await dbContext.Set<UserRole>().AddAsync(
          UserRole.Create(user.UserId, administratorRoleId),
          cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
      }
    }

    const string password = "StrongPass123!";
    using var loginClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", new
    {
      Email = user.Email,
      Password = password
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();
    var token = ExtractToken(await loginResponse.Content.ReadAsStringAsync(cancellationToken));

    user.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return user;
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);

    if (document.RootElement.TryGetProperty("value", out var valueElement) &&
        valueElement.TryGetProperty("token", out var tokenElement))
    {
      return tokenElement.GetString() ?? throw new InvalidOperationException("Token value is null.");
    }

    throw new InvalidOperationException("Token was not found in login response.");
  }
}

internal sealed record AuthenticatedTestUser(HttpClient Client, Guid UserId, string Email);

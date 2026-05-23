using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Users;

[Collection(AuthFeaturesIntegrationTestCollection.Name)]
public sealed class ConfirmUserEmailEndpointTests(AuthFeaturesWebApplicationFactory factory)
{
  [Fact]
  public async Task PostConfirmEmail_WhenAdministrator_ShouldMarkUserVerified()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var email = $"confirm-admin-{Guid.NewGuid():N}@example.com";

    using var guestClient = factory.CreateClient();
    guestClient.BaseAddress = new Uri("https://localhost");

    await guestClient.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Needs",
      LastName = "Confirm",
      Email = email,
      Password = "StrongPass123!"
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userId = await dbContext.Users
      .Where(x => x.Email == email)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{userId}/confirm-email",
      new { },
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("emailVerified", body, StringComparison.OrdinalIgnoreCase);
    Assert.True(body.Contains("\"emailVerified\":true", StringComparison.OrdinalIgnoreCase)
      || body.Contains("\"emailVerified\": true", StringComparison.OrdinalIgnoreCase));

    var updated = await dbContext.Users.SingleAsync(x => x.Id == userId, cancellationToken);
    Assert.True(updated.EmailVerified);
    Assert.NotNull(updated.EmailVerifiedAt);
  }

  [Fact]
  public async Task PostConfirmEmail_WhenAlreadyVerified_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.PostAsJsonAsync(
      $"/api/users/{admin.UserId}/confirm-email",
      new { },
      cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    Assert.Contains(UsersUtils.EmailAlreadyVerifiedMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }
}

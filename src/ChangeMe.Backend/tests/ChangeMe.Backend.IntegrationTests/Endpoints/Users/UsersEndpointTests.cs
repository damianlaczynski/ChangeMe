using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class UsersEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetUsers_WhenUserLacksPermission_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/users?pageNumber=1&pageSize=10", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task GetUsers_WhenAdministrator_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var response = await admin.Client.GetAsync("/api/users?pageNumber=1&pageSize=10", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task PostUsers_WhenAdministratorCreatesUser_ShouldReturnCreated()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);

    var userRoleId = await GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"managed-{Guid.NewGuid():N}@example.com";

    var response = await admin.Client.PostAsJsonAsync("/api/users", new
    {
      FirstName = "Managed",
      LastName = "User",
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(email, body, StringComparison.OrdinalIgnoreCase);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeMe.Backend.Infrastructure.Persistence.ApplicationDbContext>();
    var createdUser = await dbContext.Users
      .Include(x => x.AccountInvitations)
      .SingleAsync(x => x.Email == email, cancellationToken);
    Assert.False(createdUser.HasPasswordSet);
    Assert.True(createdUser.HasPendingInvitation);
    Assert.Single(createdUser.AccountInvitations);

    var fakeEmail = scope.ServiceProvider.GetRequiredService<ChangeMe.Backend.Domain.Common.IEmailService>()
      as ChangeMe.Backend.IntegrationTests.Support.Fakes.FakeEmailService;
    Assert.NotNull(fakeEmail);
    Assert.Contains(fakeEmail.SentEmails, e => e.Subject == "You're invited to ChangeMe");
  }

  private static async Task<Guid> GetRoleIdByNameAsync(
    BackendWebApplicationFactory factory,
    string roleName,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ChangeMe.Backend.Infrastructure.Persistence.ApplicationDbContext>();
    return await dbContext.Roles
      .AsNoTracking()
      .Where(x => x.Name == roleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);
  }
}

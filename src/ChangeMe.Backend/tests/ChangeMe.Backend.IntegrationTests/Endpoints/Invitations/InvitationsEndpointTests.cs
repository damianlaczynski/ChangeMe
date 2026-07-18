using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class InvitationsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetInvitations_WhenUserLacksUsersInvite_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/v1/invitations", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task CreateAndAcceptInvitation_ShouldCreateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"invite-{Guid.NewGuid():N}@example.com";

    var createResponse = await admin.Client.PostAsJsonAsync("/api/v1/invitations", new
    {
      Email = email,
      FirstName = "Invited",
      LastName = "Person",
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

    var token = await ReadInvitationTokenFromEmailAsync(factory, email, cancellationToken);
    Assert.False(string.IsNullOrWhiteSpace(token));

    using var guestClient = factory.CreateClient();
    var detailsResponse = await guestClient.GetAsync(
      $"/api/v1/invitations/accept/{token}",
      cancellationToken);

    detailsResponse.EnsureSuccessStatusCode();

    var acceptResponse = await guestClient.PostAsJsonAsync(
      $"/api/v1/invitations/accept/{token}",
      new
      {
        FirstName = "Invited",
        LastName = "Person",
        Password = "StrongPass123!"
      },
      cancellationToken);

    acceptResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userExists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    Assert.True(userExists);

    var invitation = await dbContext.Invitations
      .AsNoTracking()
      .SingleAsync(i => i.Email == email, cancellationToken);
    Assert.Equal(InvitationStatus.ACCEPTED, invitation.Status);
  }

  [Fact]
  public async Task CreateInvitation_WhenEmailAlreadyExists_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var managedUser = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);

    var response = await admin.Client.PostAsJsonAsync("/api/v1/invitations", new
    {
      Email = managedUser.Email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("A user with this email already exists.", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task CreateInvitation_WhenPendingInvitationExists_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await RolesTestHelper.GetRoleIdByNameAsync(factory, "User", cancellationToken);
    var email = $"pending-{Guid.NewGuid():N}@example.com";

    var firstResponse = await admin.Client.PostAsJsonAsync("/api/v1/invitations", new
    {
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    firstResponse.EnsureSuccessStatusCode();

    var secondResponse = await admin.Client.PostAsJsonAsync("/api/v1/invitations", new
    {
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    var body = await secondResponse.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("An invitation for this email is already pending.", body, StringComparison.Ordinal);
  }

  private static async Task<string> ReadInvitationTokenFromEmailAsync(
    BackendWebApplicationFactory factory,
    string email,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<Support.Fakes.FakeEmailService>();
    var sentEmail = fakeEmail.SentEmails.LastOrDefault(e =>
      e.Recipients.Any(r => string.Equals(r, email, StringComparison.OrdinalIgnoreCase)));

    Assert.NotNull(sentEmail);

    const string marker = "/invitations/accept/";
    var index = sentEmail.Body.IndexOf(marker, StringComparison.Ordinal);
    Assert.True(index >= 0);

    var start = index + marker.Length;
    var end = sentEmail.Body.IndexOfAny(['"', '\'', ' ', '<', '\n', '\r'], start);
    return sentEmail.Body[start..end];
  }
}

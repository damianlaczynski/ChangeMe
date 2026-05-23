using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AcceptInvitationEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task AcceptInvitation_WhenTokenIsValid_ShouldActivateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var userRoleId = await GetUserRoleIdAsync(factory, cancellationToken);
    var email = $"invite-{Guid.NewGuid():N}@example.com";

    var createResponse = await admin.Client.PostAsJsonAsync("/api/users", new
    {
      Email = email,
      RoleIds = new[] { userRoleId }
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);

    var invitationEmail = fakeEmail.SentEmails.Last(e => e.Subject == "You're invited to ChangeMe");
    var token = EmailLinkTokenExtractor.FromBody(invitationEmail.Body);
    Assert.False(string.IsNullOrWhiteSpace(token));

    using var guestClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var previewResponse = await guestClient.GetAsync($"/api/auth/invitation?token={Uri.EscapeDataString(token)}", cancellationToken);
    previewResponse.EnsureSuccessStatusCode();

    var acceptResponse = await guestClient.PostAsJsonAsync("/api/auth/accept-invitation", new
    {
      Token = token,
      FirstName = "Invited",
      LastName = "Person",
      Password = "StrongPass123!"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.True(user.HasPasswordSet);
    Assert.Equal("Invited", user.FirstName);
    Assert.Equal("Person", user.LastName);
    Assert.True(user.EmailVerified);
    Assert.NotNull(user.PasswordLastChangedAt);
  }

  private static async Task<Guid> GetUserRoleIdAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Roles
      .AsNoTracking()
      .Where(x => x.Name == "User")
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);
  }
}
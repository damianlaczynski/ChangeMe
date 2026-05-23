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
public sealed class PasswordResetEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostForgotPassword_WhenEmailUnknown_ShouldStillReturnOkWithGenericMessage()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync(
      "/api/auth/forgot-password",
      new { Email = $"unknown-{Guid.NewGuid():N}@example.com" },
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("reset link has been sent", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task ResetPassword_WhenTokenIsValid_ShouldUpdatePasswordAndRevokeSessions()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var forgotResponse = await client.PostAsJsonAsync(
      "/api/auth/forgot-password",
      new { user.Email },
      cancellationToken);

    forgotResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);

    var resetEmail = fakeEmail.SentEmails.Last(e => e.Subject == "Reset your ChangeMe password");
    var token = EmailLinkTokenExtractor.FromBody(resetEmail.Body);

    var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password", new
    {
      Token = token,
      NewPassword = "NewStrongPass456!"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var updatedUser = await dbContext.Users.SingleAsync(x => x.Id == user.UserId, cancellationToken);
    Assert.True(updatedUser.HasPasswordSet);
    Assert.NotNull(updatedUser.PasswordLastChangedAt);

    var activeSessions = await dbContext.UserSessions
      .CountAsync(x => x.UserId == user.UserId && x.RevokedAt == null, cancellationToken);
    Assert.Equal(0, activeSessions);
  }

}
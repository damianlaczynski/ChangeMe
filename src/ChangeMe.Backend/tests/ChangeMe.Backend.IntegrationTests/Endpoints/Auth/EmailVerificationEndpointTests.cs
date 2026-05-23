using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(AuthFeaturesIntegrationTestCollection.Name)]
public sealed class EmailVerificationEndpointTests(AuthFeaturesWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAuthSettings_WhenAuthFeaturesEnabled_ShouldExposeVerificationFlag()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.GetAsync("/api/auth/settings", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var settings = await IntegrationApiJson.ReadValueAsync<AuthSettingsDto>(response.Content, cancellationToken);
    Assert.NotNull(settings);
    Assert.True(settings!.EmailVerificationEnabled);
  }

  [Fact]
  public async Task PostRegister_WhenEmailVerificationEnabled_ShouldRequireVerificationWithoutSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"verify-register-{Guid.NewGuid():N}@example.com";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Verify",
      LastName = "Register",
      Email = email,
      Password = "StrongPass123!"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await IntegrationApiJson.ReadValueAsync<RegisterUserResponseDto>(response.Content, cancellationToken);
    Assert.NotNull(body);
    Assert.True(body!.RequiresEmailVerification);
    Assert.Null(body.AuthSession);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.False(user.EmailVerified);

    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);
    Assert.Contains(fakeEmail.SentEmails, e => e.Subject == "Verify your ChangeMe email");
  }

  [Fact]
  public async Task PostLogin_WhenEmailIsUnverified_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"verify-login-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Verify",
      LastName = "Login",
      Email = email,
      Password = password
    }, cancellationToken);

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    Assert.Contains(AuthSessionUtils.EmailNotVerifiedMessage, responseBody, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task VerifyEmail_WhenTokenIsValid_ShouldAllowSignIn()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"verify-flow-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Verify",
      LastName = "Flow",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);

    var verificationEmail = fakeEmail.SentEmails.Last(e => e.Subject == "Verify your ChangeMe email");
    var token = EmailLinkTokenExtractor.FromBody(verificationEmail.Body);
    Assert.False(string.IsNullOrWhiteSpace(token));

    var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-email", new { Token = token }, cancellationToken);
    Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
  }
}
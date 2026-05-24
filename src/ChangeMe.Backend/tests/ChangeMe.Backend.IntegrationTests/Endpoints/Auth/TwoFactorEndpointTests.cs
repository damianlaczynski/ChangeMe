using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(TwoFactorRequiredIntegrationTestCollection.Name)]
public sealed class TwoFactorRequiredEndpointTests(TwoFactorRequiredWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenTwoFactorRequiredAndNotEnrolled_ShouldReturnSetupRequiredSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"2fa-setup-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Setup",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);

    loginResponse.EnsureSuccessStatusCode();
    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    Assert.NotNull(login?.AuthSession);
    Assert.True(login.AuthSession!.TwoFactorSetupRequired);
  }
}

[Collection(TwoFactorIntegrationTestCollection.Name)]
public sealed class TwoFactorEndpointTests(TwoFactorWebApplicationFactory factory)
{
  [Fact]
  public async Task PostLogin_WhenTwoFactorEnabled_ShouldReturnPendingChallengeWithoutToken()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"2fa-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";
    var secret = await SeedUserWithTwoFactorAsync(factory, email, password, cancellationToken);

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    Assert.Null(login?.AuthSession);
    Assert.NotNull(login?.TwoFactorChallenge);

    var verifyResponse = await client.PostAsJsonAsync("/api/auth/two-factor/verify", new
    {
      ChallengeId = login!.TwoFactorChallenge!.ChallengeId,
      VerificationCode = ComputeTotp(secret)
    }, cancellationToken);

    verifyResponse.EnsureSuccessStatusCode();
    var auth = await IntegrationApiJson.ReadValueAsync<AuthResponseDto>(verifyResponse.Content, cancellationToken);
    Assert.NotNull(auth?.Token);
    Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
  }

  [Fact]
  public async Task PostTwoFactorSetupConfirm_WhenEnrollmentPending_ShouldEnableTwoFactor()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"2fa-setup-flow-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Setup",
      LastName = "Flow",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    var token = login!.AuthSession!.Token;
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/two-factor/setup/begin",
      new { CurrentPassword = password },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<BeginTwoFactorSetupResponseDto>(
      beginResponse.Content,
      cancellationToken);

    var confirmResponse = await client.PostAsJsonAsync(
      "/api/auth/two-factor/setup/confirm",
      new { VerificationCode = ComputeTotp(begin!.SharedSecret) },
      cancellationToken);
    confirmResponse.EnsureSuccessStatusCode();

    var completed = await IntegrationApiJson.ReadValueAsync<TwoFactorSetupCompletedDto>(
      confirmResponse.Content,
      cancellationToken);
    Assert.NotNull(completed);
    Assert.Equal(10, completed!.RecoveryCodes.Count);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.True(user.TwoFactorEnabled);
    Assert.False(await dbContext.TwoFactorEnrollmentPending.AnyAsync(x => x.UserId == user.Id, cancellationToken));
  }

  [Fact]
  public async Task PostDisableTwoFactor_WhenEnabledWithRecoveryCodes_ShouldDisableAndClearCodes()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var email = $"2fa-disable-{Guid.NewGuid():N}@example.com";
    const string password = "StrongPass123!";

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Disable",
      LastName = "User",
      Email = email,
      Password = password
    }, cancellationToken);

    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      Email = email,
      Password = password,
    }, cancellationToken);
    loginResponse.EnsureSuccessStatusCode();

    var login = await IntegrationApiJson.ReadValueAsync<LoginResponseDto>(loginResponse.Content, cancellationToken);
    client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.AuthSession!.Token);

    var beginResponse = await client.PostAsJsonAsync(
      "/api/auth/two-factor/setup/begin",
      new { CurrentPassword = password },
      cancellationToken);
    beginResponse.EnsureSuccessStatusCode();

    var begin = await IntegrationApiJson.ReadValueAsync<BeginTwoFactorSetupResponseDto>(
      beginResponse.Content,
      cancellationToken);

    var confirmResponse = await client.PostAsJsonAsync(
      "/api/auth/two-factor/setup/confirm",
      new { VerificationCode = ComputeTotp(begin!.SharedSecret) },
      cancellationToken);
    confirmResponse.EnsureSuccessStatusCode();

    var disableResponse = await client.PostAsJsonAsync(
      "/api/auth/two-factor/disable",
      new
      {
        CurrentPassword = password,
        VerificationCode = ComputeTotp(begin.SharedSecret)
      },
      cancellationToken);
    disableResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    Assert.False(user.TwoFactorEnabled);
    Assert.False(await dbContext.UserRecoveryCodes.AnyAsync(x => x.UserId == user.Id, cancellationToken));
  }

  private static async Task<string> SeedUserWithTwoFactorAsync(
    TwoFactorWebApplicationFactory factory,
    string email,
    string password,
    CancellationToken cancellationToken)
  {
    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await client.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Two",
      LastName = "Factor",
      Email = email,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var totpService = scope.ServiceProvider.GetRequiredService<ITotpService>();
    var protector = scope.ServiceProvider.GetRequiredService<ITwoFactorSecretProtector>();

    var user = await dbContext.Users.SingleAsync(x => x.Email == email, cancellationToken);
    var secret = totpService.GenerateSecret();
    user.EnableTwoFactor(protector.Protect(secret), DateTime.UtcNow);
    await dbContext.SaveChangesAsync(cancellationToken);
    return secret;
  }

  private static string ComputeTotp(string secret)
  {
    var totp = new Totp(Base32Encoding.ToBytes(secret), step: 30, totpSize: 6);
    return totp.ComputeTotp(DateTime.UtcNow);
  }
}

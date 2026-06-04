using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Common;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using ChangeMe.Backend.IntegrationTests.Support.Fakes;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class EmailChangeEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostEmailChange_WhenStepUpValid_ShouldCreatePendingChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var newEmail = $"new-{Guid.NewGuid():N}@example.com";

    var response = await user.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = newEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
    var account = await IntegrationApiJson.ReadValueAsync<MyAccountDto>(response.Content, cancellationToken);
    Assert.NotNull(account?.PendingEmailChange);
    Assert.Equal(newEmail, account.PendingEmailChange!.NewEmail);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasToken = await db.UserAuthTokens.AnyAsync(
      x => x.UserId == user.UserId && x.Type == UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    Assert.True(hasToken);
  }

  [Fact]
  public async Task PostUsersCancelPendingEmailChange_WhenAdministrator_ShouldClearPending()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var newEmail = $"pending-{Guid.NewGuid():N}@example.com";

    await user.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = newEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var cancelResponse = await admin.Client.PostAsJsonAsync(
      $"/api/users/{user.UserId}/cancel-pending-email-change",
      new { },
      cancellationToken);
    cancelResponse.EnsureSuccessStatusCode();

    var details = await IntegrationApiJson.ReadValueAsync<UserDetailsDto>(
      cancelResponse.Content,
      cancellationToken);
    Assert.Null(details?.PendingEmailChange);

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var entity = await db.Users.AsNoTracking().SingleAsync(x => x.Id == user.UserId, cancellationToken);
    Assert.False(entity.HasPendingEmailChange);
  }

  [Fact]
  public async Task PostEmailChangeResend_WhenPendingChange_ShouldPersistTokenUsableForConfirm()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var newEmail = $"resend-{Guid.NewGuid():N}@example.com";

    await user.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = newEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    var resendResponse = await user.Client.PostAsJsonAsync(
      "/api/auth/email-change/resend",
      new { },
      cancellationToken);
    resendResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);

    var confirmEmail = fakeEmail.SentEmails.Last(e =>
      e.Subject == "Confirm your new ChangeMe email address");
    var token = EmailLinkTokenExtractor.FromBody(confirmEmail.Body);
    Assert.False(string.IsNullOrWhiteSpace(token));

    using var guestClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var confirmResponse = await guestClient.PostAsJsonAsync("/api/auth/email-change/confirm", new
    {
      Token = token
    }, cancellationToken);
    confirmResponse.EnsureSuccessStatusCode();

    var result = await IntegrationApiJson.ReadValueAsync<ConfirmEmailChangeResponseDto>(
      confirmResponse.Content,
      cancellationToken);
    Assert.NotNull(result);
    Assert.True(result!.Succeeded);

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var entity = await db.Users.AsNoTracking().SingleAsync(x => x.Id == user.UserId, cancellationToken);
    Assert.Equal(newEmail, entity.Email);
    Assert.False(entity.HasPendingEmailChange);
  }

  [Fact]
  public async Task PostRegister_WhenAnotherUserHasPendingChangeToSameEmail_ShouldSucceed()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var targetEmail = $"target-{Guid.NewGuid():N}@example.com";
    var changer = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await changer.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = targetEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    using var guestClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var registerResponse = await guestClient.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "New",
      LastName = "Owner",
      Email = targetEmail,
      Password = password
    }, cancellationToken);

    registerResponse.EnsureSuccessStatusCode();

    await using var scope = factory.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Assert.True(await db.Users.AnyAsync(x => x.Email == targetEmail, cancellationToken));
  }

  [Fact]
  public async Task PostConfirmEmailChange_WhenProfileEmailTakenSinceRequest_ShouldFailWithoutApplyingChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    const string password = "StrongPass123!";
    var targetEmail = $"confirm-race-{Guid.NewGuid():N}@example.com";
    var changer = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await changer.Client.PostAsJsonAsync("/api/auth/email-change", new
    {
      NewEmail = targetEmail,
      CurrentPassword = password,
      VerificationCode = (string?)null
    }, cancellationToken);

    using var guestClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    await guestClient.PostAsJsonAsync("/api/auth/register", new
    {
      FirstName = "Blocking",
      LastName = "Owner",
      Email = targetEmail,
      Password = password
    }, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var fakeEmail = scope.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
    Assert.NotNull(fakeEmail);

    var confirmEmail = fakeEmail.SentEmails.Last(e =>
      e.Subject == "Confirm your new ChangeMe email address");
    var token = EmailLinkTokenExtractor.FromBody(confirmEmail.Body);
    Assert.False(string.IsNullOrWhiteSpace(token));

    var confirmResponse = await guestClient.PostAsJsonAsync("/api/auth/email-change/confirm", new
    {
      Token = token
    }, cancellationToken);
    confirmResponse.EnsureSuccessStatusCode();

    var result = await IntegrationApiJson.ReadValueAsync<ConfirmEmailChangeResponseDto>(
      confirmResponse.Content,
      cancellationToken);
    Assert.NotNull(result);
    Assert.False(result!.Succeeded);
    Assert.Equal(EmailChangeUtils.TargetEmailTakenMessage, result.Message);

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var changerEntity = await db.Users.AsNoTracking().SingleAsync(x => x.Id == changer.UserId, cancellationToken);
    Assert.NotEqual(targetEmail, changerEntity.Email);
    Assert.True(changerEntity.HasPendingEmailChange);
    Assert.Equal(targetEmail, changerEntity.PendingNewEmail);
  }
}

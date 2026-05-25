using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class UserAuthTokenServiceTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task IssueTokenAsync_ThenValidate_ShouldReturnUserId()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var authenticated = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var tokenService = scope.ServiceProvider.GetRequiredService<IUserAuthTokenService>();

    var issueResult = await tokenService.IssueTokenAsync(
      authenticated.UserId,
      UserAuthTokenType.PasswordReset,
      cancellationToken);

    Assert.True(issueResult.IsSuccess);
    Assert.False(string.IsNullOrWhiteSpace(issueResult.Value));

    var validateResult = await tokenService.ValidateTokenAsync(
      issueResult.Value,
      UserAuthTokenType.PasswordReset,
      cancellationToken);

    Assert.True(validateResult.IsSuccess);
    Assert.Equal(authenticated.UserId, validateResult.Value);
  }

  [Fact]
  public async Task IssueTokenAsync_WhenReissued_ShouldInvalidatePreviousToken()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var authenticated = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var tokenService = scope.ServiceProvider.GetRequiredService<IUserAuthTokenService>();

    var first = await tokenService.IssueTokenAsync(
      authenticated.UserId,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    var second = await tokenService.IssueTokenAsync(
      authenticated.UserId,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    var firstValidation = await tokenService.ValidateTokenAsync(
      first.Value,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    var secondValidation = await tokenService.ValidateTokenAsync(
      second.Value,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    Assert.False(firstValidation.IsSuccess);
    Assert.True(secondValidation.IsSuccess);
  }

  [Fact]
  public async Task GetActiveUnusedTokenExpiresAtUtcAsync_WhenTokenExpired_ReturnsExpiresAtUtc()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var authenticated = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var tokenService = scope.ServiceProvider.GetRequiredService<IUserAuthTokenService>();

    var issueResult = await tokenService.IssueTokenAsync(
      authenticated.UserId,
      UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.True(issueResult.IsSuccess);

    var token = await context.UserAuthTokens
      .SingleAsync(x => x.UserId == authenticated.UserId && x.Type == UserAuthTokenType.Invitation, cancellationToken);

    var expiredAtUtc = DateTime.UtcNow.AddHours(-1);
    context.Entry(token).Property(x => x.ExpiresAtUtc).CurrentValue = expiredAtUtc;
    await context.SaveChangesAsync(cancellationToken);

    var expiresAtUtc = await tokenService.GetActiveUnusedTokenExpiresAtUtcAsync(
      authenticated.UserId,
      UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.Equal(expiredAtUtc, expiresAtUtc);
  }
}

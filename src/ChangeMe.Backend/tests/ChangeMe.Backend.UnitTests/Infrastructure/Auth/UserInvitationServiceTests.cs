using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class UserInvitationServiceTests
{
  [Fact]
  public async Task SendInvitationAsync_WhenEmailDeliveryFails_ReturnsErrorWithoutPendingInvitation()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(SendInvitationAsync_WhenEmailDeliveryFails_ReturnsErrorWithoutPendingInvitation));

    var user = User.CreateInvited("invite@example.com").Value;
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var tokenService = CreateTokenService(context);
    var authEmailService = new FailingAuthEmailService();
    var sut = new UserInvitationService(
      context,
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var result = await sut.SendInvitationAsync(user, cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains(UserInvitationService.InvitationEmailDeliveryFailedMessage, result.Errors);
    Assert.False(user.HasPendingInvitation);

    Assert.NotNull(authEmailService.LastPlainToken);
    var tokenValidation = await tokenService.ValidateTokenAsync(
      authEmailService.LastPlainToken,
      UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.False(tokenValidation.IsSuccess);
  }

  [Fact]
  public async Task SendInvitationAsync_WhenEmailDeliverySucceeds_RecordsPendingInvitation()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(SendInvitationAsync_WhenEmailDeliverySucceeds_RecordsPendingInvitation));

    var user = User.CreateInvited("invite@example.com").Value;
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var tokenService = CreateTokenService(context);
    var authEmailService = new FakeAuthEmailService();
    var sut = new UserInvitationService(
      context,
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var result = await sut.SendInvitationAsync(user, cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.True(user.HasPendingInvitation);

    var pending = user.AccountInvitations.Single(x => x.IsPending);
    var token = await context.UserAuthTokens.SingleAsync(
      x => x.UserId == user.Id && x.Type == UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.Equal(pending.LinkExpiresAtUtc, token.ExpiresAtUtc);
  }

  private static UserAuthTokenService CreateTokenService(ApplicationDbContext context) =>
    new(
      context,
      TestAuthOptions.Create(),
      TimeProvider.System);

  private sealed class FailingAuthEmailService : IAuthEmailService
  {
    public string? LastPlainToken { get; private set; }

    public Task<Result> SendAccountInvitationAsync(
      User user,
      string plainToken,
      CancellationToken cancellationToken = default)
    {
      LastPlainToken = plainToken;
      return Task.FromResult(Result.Error());
    }

    public Task<Result> SendPasswordResetRequestedAsync(
      User user,
      string plainToken,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendPasswordResetCompletedAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendPasswordChangedAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendVerifyEmailAsync(
      User user,
      string plainToken,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendTwoFactorEnabledAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendTwoFactorDisabledAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendTwoFactorResetByAdminAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendRecoveryCodeUsedAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendExternalAccountLinkedAsync(
      User user,
      string providerDisplayName,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendExternalAccountUnlinkedAsync(
      User user,
      string providerDisplayName,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());
  }
}

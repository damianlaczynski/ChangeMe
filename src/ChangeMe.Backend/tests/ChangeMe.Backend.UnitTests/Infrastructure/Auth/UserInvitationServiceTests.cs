using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
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
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var result = await sut.SendInvitationAsync(user, cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains("The email could not be sent. Please try again.", result.Errors);
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
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var result = await sut.SendInvitationAsync(user, cancellationToken);

    Assert.True(result.IsSuccess);
    await context.SaveChangesAsync(cancellationToken);
    Assert.True(user.HasPendingInvitation);

    var pending = user.AccountInvitations.Single(x => x.IsPending);
    var token = await context.UserAuthTokens.SingleAsync(
      x => x.UserId == user.Id && x.Type == UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.Equal(pending.LinkExpiresAtUtc, token.ExpiresAtUtc);
  }

  [Fact]
  public async Task CancelInvitationAsync_WhenTokenInvalidationFails_DoesNotPersistInvitationRevocation()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(CancelInvitationAsync_WhenTokenInvalidationFails_DoesNotPersistInvitationRevocation));

    var user = User.CreateInvited("invite@example.com").Value;
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var tokenService = CreateTokenService(context);
    var authEmailService = new FakeAuthEmailService();
    var sendSut = new UserInvitationService(
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var sendResult = await sendSut.SendInvitationAsync(user, cancellationToken);
    Assert.True(sendResult.IsSuccess);
    await context.SaveChangesAsync(cancellationToken);

    var plainToken = authEmailService.LastPlainToken!;
    var cancelSut = new UserInvitationService(
      new ThrowingInvalidateTokenService(tokenService),
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => cancelSut.CancelInvitationAsync(user, cancellationToken));

    context.ChangeTracker.Clear();
    var reloadedUser = await context.Users
      .Include(x => x.AccountInvitations)
      .SingleAsync(x => x.Id == user.Id, cancellationToken);
    Assert.True(reloadedUser.HasPendingInvitation);

    var tokenValidation = await tokenService.ValidateTokenAsync(
      plainToken,
      UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.True(tokenValidation.IsSuccess);
  }

  [Fact]
  public async Task CancelInvitationAsync_WhenSuccessful_RevokesInvitationAndInvalidatesToken()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(CancelInvitationAsync_WhenSuccessful_RevokesInvitationAndInvalidatesToken));

    var user = User.CreateInvited("invite@example.com").Value;
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var tokenService = CreateTokenService(context);
    var authEmailService = new FakeAuthEmailService();
    var sut = new UserInvitationService(
      tokenService,
      authEmailService,
      TestAuthOptions.Create(),
      TimeProvider.System);

    var sendResult = await sut.SendInvitationAsync(user, cancellationToken);
    Assert.True(sendResult.IsSuccess);
    await context.SaveChangesAsync(cancellationToken);

    var plainToken = authEmailService.LastPlainToken!;

    var cancelResult = await sut.CancelInvitationAsync(user, cancellationToken);
    Assert.True(cancelResult.IsSuccess);
    await context.SaveChangesAsync(cancellationToken);
    Assert.False(user.HasPendingInvitation);

    var tokenValidation = await tokenService.ValidateTokenAsync(
      plainToken,
      UserAuthTokenType.Invitation,
      cancellationToken);

    Assert.False(tokenValidation.IsSuccess);
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
      return Task.FromResult(Result.Error("The email could not be sent. Please try again."));
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

    public Task<Result> SendPasskeyAddedAsync(
      User user,
      string passkeyName,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendPasskeyRemovedAsync(
      User user,
      string passkeyName,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendPasskeysResetByAdminAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendEmailChangeRequestedAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendConfirmEmailChangeAsync(
      string newEmail,
      string plainToken,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendEmailChangeCancelledAsync(
      User user,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendEmailChangeCompletedAsync(
      string previousEmail,
      string newEmail,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());

    public Task<Result> SendEmailChangedByAdminAsync(
      string previousEmail,
      string newEmail,
      CancellationToken cancellationToken = default) =>
      Task.FromResult(Result.Success());
  }

  private sealed class ThrowingInvalidateTokenService(IUserAuthTokenService inner) : IUserAuthTokenService
  {
    public Task<DateTime?> GetActiveUnusedTokenExpiresAtUtcAsync(
      Guid userId,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      inner.GetActiveUnusedTokenExpiresAtUtcAsync(userId, type, cancellationToken);

    public Task<Result<string>> IssueTokenAsync(
      Guid userId,
      UserAuthTokenType type,
      DateTime? issuedAtUtc = null,
      CancellationToken cancellationToken = default) =>
      inner.IssueTokenAsync(userId, type, issuedAtUtc, cancellationToken);

    public Task<Result<Guid>> ValidateTokenAsync(
      string plainToken,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      inner.ValidateTokenAsync(plainToken, type, cancellationToken);

    public Task MarkTokenUsedAsync(string plainToken, CancellationToken cancellationToken = default) =>
      inner.MarkTokenUsedAsync(plainToken, cancellationToken);

    public Task InvalidateUnusedTokensAsync(
      Guid userId,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      throw new InvalidOperationException("Simulated token invalidation failure.");
  }
}

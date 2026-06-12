using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class RequestEmailChangeHandlerTests
{
  [Fact]
  public async Task Handle_WhenNotificationEmailFails_ShouldReturnErrorButPersistPendingChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenNotificationEmailFails_ShouldReturnErrorButPersistPendingChange));
    var authOptions = TestAuthOptions.Create();
    var passwordHasher = new PasswordHasherAdapter();
    var currentEmail = "current@example.com";
    var newEmail = "new@example.com";

    var user = User.CreateWithPassword(
      "Test",
      "User",
      currentEmail,
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: true).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new RequestEmailChangeHandler(
      new UnusedTestMediator(),
      context,
      passwordHasher,
      new FakeTotpService(),
      new FakeTwoFactorSecretProtector(),
      new FakeRecoveryCodeHasher(),
      new FakeAuthEmailService { FailSendEmailChangeRequested = true },
      new UserAuthTokenService(context, authOptions, TimeProvider.System),
      new FakeUserAccessor { UserId = user.Id },
      authOptions,
      new PasskeyPolicyEvaluator(authOptions),
      TimeProvider.System,
      NullLogger<RequestEmailChangeHandler>.Instance);

    var result = await handler.Handle(
      new RequestEmailChangeCommand(newEmail, "StrongPass123!", null),
      cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains(
      FailingAuthEmailService.DefaultErrorMessage,
      result.Errors.First());

    context.ChangeTracker.Clear();
    var updated = await context.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, cancellationToken);
    Assert.True(updated.HasPendingEmailChange);
    Assert.Equal(newEmail, updated.PendingNewEmail);

    var hasToken = await context.UserAuthTokens.AnyAsync(
      x => x.UserId == user.Id && x.Type == UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    Assert.True(hasToken);
  }
}

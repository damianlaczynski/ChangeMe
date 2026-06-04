using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class ConfirmEmailChangeHandlerTests
{
  [Fact]
  public async Task Handle_WhenCompletionEmailFails_ShouldStillReturnSuccessAndApplyChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenCompletionEmailFails_ShouldStillReturnSuccessAndApplyChange));
    var authOptions = TestAuthOptions.Create();
    var tokenService = new UserAuthTokenService(context, authOptions, TimeProvider.System);
    var passwordHasher = new PasswordHasherAdapter();
    var previousEmail = "old@example.com";
    var newEmail = "new@example.com";

    var user = User.CreateWithPassword(
      "Test",
      "User",
      previousEmail,
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: true).Value;
    user.BeginPendingEmailChange(newEmail, DateTime.UtcNow);

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var plainToken = (await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken: cancellationToken)).Value;
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ConfirmEmailChangeHandler(
      context,
      new FakeAuthEmailService { FailSendEmailChangeCompleted = true },
      tokenService,
      new FakeUserAccessor(),
      TimeProvider.System,
      NullLogger<ConfirmEmailChangeHandler>.Instance);

    var result = await handler.Handle(new ConfirmEmailChangeCommand(plainToken), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.True(result.Value!.Succeeded);
    Assert.Equal(EmailChangeUtils.EmailChangeConfirmedMessage, result.Value.Message);

    context.ChangeTracker.Clear();
    var updated = await context.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, cancellationToken);
    Assert.Equal(newEmail, updated.Email);
    Assert.False(updated.HasPendingEmailChange);
    Assert.True(updated.EmailVerified);
  }
}

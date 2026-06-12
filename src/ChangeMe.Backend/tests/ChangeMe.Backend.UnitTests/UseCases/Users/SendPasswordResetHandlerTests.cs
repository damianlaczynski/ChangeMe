using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class SendPasswordResetHandlerTests
{
  [Fact]
  public async Task Handle_WhenUserHasNoLocalPassword_ShouldReturnErrorWithoutSendingReset()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserHasNoLocalPassword_ShouldReturnErrorWithoutSendingReset));

    var user = User.CreateInvited("external-only@example.com", "External", "Only").Value;
    var login = ExternalLogin.Create(user.Id, "google", "subject-1").Value;
    user.AddExternalLogin(login);

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context);

    var result = await handler.Handle(new SendPasswordResetCommand(user.Id), cancellationToken);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains(UsersUtils.CannotSendPasswordResetWithoutLocalPasswordMessage, result.Errors.First());
    Assert.False(await context.UserAuthTokens.AnyAsync(cancellationToken));
  }

  [Fact]
  public async Task Handle_WhenUserHasLocalPassword_ShouldSendPasswordReset()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserHasLocalPassword_ShouldSendPasswordReset));

    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Local",
      "User",
      "local-user@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context);

    var result = await handler.Handle(new SendPasswordResetCommand(user.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.True(await context.UserAuthTokens.AnyAsync(
      x => x.UserId == user.Id && x.Type == UserAuthTokenType.PasswordReset,
      cancellationToken));
  }

  private static SendPasswordResetHandler CreateHandler(ApplicationDbContext context)
  {
    var tokenService = new UserAuthTokenService(
      context,
      TestAuthOptions.Create(),
      TimeProvider.System);

    return new SendPasswordResetHandler(
      new GetUserByIdDispatchingTestMediator(context),
      context,
      new UserPasswordResetService(tokenService, new FakeAuthEmailService()));
  }
}

using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class ConfirmUserEmailHandlerTests
{
  [Fact]
  public async Task Handle_WhenUserIsUnverified_ShouldMarkEmailVerified()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsUnverified_ShouldMarkEmailVerified));
    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "admin-confirm@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ConfirmUserEmailHandler(context, new GetUserByIdDispatchingTestMediator(context));
    var result = await handler.Handle(new ConfirmUserEmailCommand(user.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.EmailVerified);

    var updated = await context.Users.FindAsync([user.Id], cancellationToken);
    Assert.True(updated!.EmailVerified);
    Assert.NotNull(updated.EmailVerifiedAt);
  }

  [Fact]
  public async Task Handle_WhenEmailAlreadyVerified_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailAlreadyVerified_ShouldReturnConflict));
    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "verified@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ConfirmUserEmailHandler(context, new GetUserByIdDispatchingTestMediator(context));
    var result = await handler.Handle(new ConfirmUserEmailCommand(user.Id), cancellationToken);

    Assert.Equal(ResultStatus.Conflict, result.Status);
    Assert.Contains(UsersUtils.EmailAlreadyVerifiedMessage, result.Errors.First());
  }
}

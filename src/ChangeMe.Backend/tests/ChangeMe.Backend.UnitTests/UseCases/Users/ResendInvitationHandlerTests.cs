using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class ResendInvitationHandlerTests
{
  [Fact]
  public async Task Handle_WhenUserIsDeactivated_ReturnsError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsDeactivated_ReturnsError));

    var user = User.CreateInvited("invite@example.com").Value;
    user.Deactivate();
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ResendInvitationHandler(
      new UnusedTestMediator(),
      context,
      null!);

    var result = await handler.Handle(new ResendInvitationCommand(user.Id), cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains(UsersUtils.CannotManageInvitationForDeactivatedMessage, result.Errors);
  }

  [Fact]
  public async Task Handle_WhenAccountWasNotInvited_ReturnsError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenAccountWasNotInvited_ReturnsError));

    var user = User.CreateInvited("google@example.com").Value;
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ResendInvitationHandler(
      new UnusedTestMediator(),
      context,
      null!);

    var result = await handler.Handle(new ResendInvitationCommand(user.Id), cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains(UsersUtils.NoPendingInvitationMessage, result.Errors);
  }
}

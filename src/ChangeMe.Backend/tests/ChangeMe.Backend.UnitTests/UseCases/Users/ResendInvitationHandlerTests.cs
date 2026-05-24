using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;
using MediatR;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class ResendInvitationHandlerTests
{
  [Fact]
  public async Task Handle_WhenUserIsDeactivated_ReturnsError()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsDeactivated_ReturnsError));

    var user = User.CreateInvited("invite@example.com").Value;
    user.Deactivate();
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();

    var handler = new ResendInvitationHandler(
      new UnusedMediator(),
      context,
      null!);

    var result = await handler.Handle(new ResendInvitationCommand(user.Id), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(UsersUtils.CannotResendInvitationToDeactivatedMessage, result.Errors);
  }

  [Fact]
  public async Task Handle_WhenAccountWasNotInvited_ReturnsError()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenAccountWasNotInvited_ReturnsError));

    var user = User.CreateInvited("google@example.com").Value;
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();

    var handler = new ResendInvitationHandler(
      new UnusedMediator(),
      context,
      null!);

    var result = await handler.Handle(new ResendInvitationCommand(user.Id), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Contains(UsersUtils.AccountWasNotInvitedMessage, result.Errors);
  }

  private sealed class UnusedMediator : IMediator, IPublisher
  {
    public Task<TResponse> Send<TResponse>(
      IRequest<TResponse> request,
      CancellationToken cancellationToken = default) =>
      throw new InvalidOperationException();

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest =>
      Task.CompletedTask;

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
      throw new InvalidOperationException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default) =>
      throw new InvalidOperationException();

    public IAsyncEnumerable<object?> CreateStream(
      object request,
      CancellationToken cancellationToken = default) =>
      throw new InvalidOperationException();

    public Task Publish(object notification, CancellationToken cancellationToken = default) =>
      Task.CompletedTask;

    public Task Publish<TNotification>(
      TNotification notification,
      CancellationToken cancellationToken = default)
      where TNotification : INotification =>
      Task.CompletedTask;
  }
}

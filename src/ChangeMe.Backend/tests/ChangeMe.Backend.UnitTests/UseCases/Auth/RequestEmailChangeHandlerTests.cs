using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class RequestEmailChangeHandlerTests
{
  [Fact]
  public async Task Handle_WhenNotificationEmailFails_ShouldReturnErrorWithoutPersistingPendingChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenNotificationEmailFails_ShouldReturnErrorWithoutPersistingPendingChange));
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
      new UnusedMediator(),
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
      TimeProvider.System);

    var result = await handler.Handle(
      new RequestEmailChangeCommand(newEmail, "StrongPass123!", null),
      cancellationToken);

    Assert.False(result.IsSuccess);
    Assert.Contains(
      FailingAuthEmailService.DefaultErrorMessage,
      result.Errors.First());

    context.ChangeTracker.Clear();
    var updated = await context.Users.AsNoTracking().SingleAsync(x => x.Id == user.Id, cancellationToken);
    Assert.False(updated.HasPendingEmailChange);
    Assert.Equal(currentEmail, updated.Email);

    var hasToken = await context.UserAuthTokens.AnyAsync(
      x => x.UserId == user.Id && x.Type == UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    Assert.False(hasToken);
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

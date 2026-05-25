using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;
using MediatR;

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

    var handler = new ConfirmUserEmailHandler(context, new StubMediator(context));
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

    var handler = new ConfirmUserEmailHandler(context, new StubMediator(context));
    var result = await handler.Handle(new ConfirmUserEmailCommand(user.Id), cancellationToken);

    Assert.Equal(ResultStatus.Conflict, result.Status);
    Assert.Contains(UsersUtils.EmailAlreadyVerifiedMessage, result.Errors.First());
  }

  private sealed class StubUserAuthTokenService : IUserAuthTokenService
  {
    public Task<DateTime?> GetActiveUnusedTokenExpiresAtUtcAsync(
      Guid userId,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      Task.FromResult<DateTime?>(null);

    public Task<Result<string>> IssueTokenAsync(
      Guid userId,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task<Result<Guid>> ValidateTokenAsync(
      string plainToken,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task MarkTokenUsedAsync(string plainToken, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task InvalidateUnusedTokensAsync(
      Guid userId,
      UserAuthTokenType type,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }

  private sealed class StubMediator(ApplicationDbContext context) : IMediator, IPublisher
  {
    public async Task<TResponse> Send<TResponse>(
      IRequest<TResponse> request,
      CancellationToken cancellationToken = default)
    {
      if (request is GetUserByIdQuery getUserQuery)
      {
        var handler = new GetUserByIdHandler(
          context,
          new PasswordExpirationEvaluator(TestAuthOptions.Create()),
          new StubUserAuthTokenService(),
          TestAuthOptions.Create(),
          new PasskeyPolicyEvaluator(TestAuthOptions.Create()),
          TimeProvider.System);
        var result = await handler.Handle(getUserQuery, cancellationToken);
        return (TResponse)(object)result;
      }

      throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest =>
      Task.CompletedTask;

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(
      object request,
      CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task Publish(object notification, CancellationToken cancellationToken = default) =>
      Task.CompletedTask;

    public Task Publish<TNotification>(
      TNotification notification,
      CancellationToken cancellationToken = default)
      where TNotification : INotification =>
      Task.CompletedTask;
  }
}

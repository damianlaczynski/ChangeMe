using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UseCases.Users;
using Mediator;

namespace ChangeMe.Backend.UnitTests.Support;

internal abstract class TestMediatorBase : IMediator
{
  public abstract ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default);

  public ValueTask<TResponse> Send<TResponse>(
    ICommand<TResponse> command,
    CancellationToken cancellationToken = default) =>
    Send((IRequest<TResponse>)command, cancellationToken);

  public ValueTask<TResponse> Send<TResponse>(
    IQuery<TResponse> query,
    CancellationToken cancellationToken = default) =>
    Send((IRequest<TResponse>)query, cancellationToken);

  public virtual ValueTask<object?> Send(object message, CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamQuery<TResponse> query,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamRequest<TResponse> request,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamCommand<TResponse> command,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public virtual IAsyncEnumerable<object?> CreateStream(
    object message,
    CancellationToken cancellationToken = default) =>
    throw new NotSupportedException();

  public ValueTask Publish<TNotification>(
    TNotification notification,
    CancellationToken cancellationToken = default)
    where TNotification : INotification => default;

  public ValueTask Publish(object notification, CancellationToken cancellationToken = default) => default;
}

internal sealed class UnusedTestMediator : TestMediatorBase
{
  public override ValueTask<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default) =>
    throw new InvalidOperationException();
}

internal sealed class GetUserByIdDispatchingTestMediator(ApplicationDbContext context) : TestMediatorBase
{
  public override async ValueTask<TResponse> Send<TResponse>(
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
      return (TResponse)(object)result!;
    }

    throw new NotSupportedException($"Unsupported request: {request.GetType().Name}");
  }
}

internal sealed class StubUserAuthTokenService : IUserAuthTokenService
{
  public Task<DateTime?> GetActiveUnusedTokenExpiresAtUtcAsync(
    Guid userId,
    UserAuthTokenType type,
    CancellationToken cancellationToken = default) =>
    Task.FromResult<DateTime?>(null);

  public Task<Result<string>> IssueTokenAsync(
    Guid userId,
    UserAuthTokenType type,
    DateTime? issuedAtUtc = null,
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

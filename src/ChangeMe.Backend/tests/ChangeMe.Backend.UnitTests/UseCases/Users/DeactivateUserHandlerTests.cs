using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class DeactivateUserHandlerTests
{
  [Fact]
  public async Task Handle_WhenLastActiveAdministrator_ShouldReturnError()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenLastActiveAdministrator_ShouldReturnError));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var administratorRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var admin = User.CreateWithPassword(
      "Admin",
      "User",
      "only-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    admin.AssignRole(administratorRoleId);

    var actingAdminId = Guid.NewGuid();
    await context.Users.AddAsync(admin, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new StubMediator(context),
      context,
      new FakeUserAccessor { UserId = actingAdminId });

    var result = await handler.Handle(new DeactivateUserCommand(admin.Id), cancellationToken);

    Assert.Equal(ResultStatus.Error, result.Status);
    Assert.Contains(UsersUtils.CannotDeactivateLastAdministratorMessage, result.Errors.First());
    Assert.True((await context.Users.FindAsync([admin.Id], cancellationToken))!.IsActive);
  }

  [Fact]
  public async Task Handle_WhenAnotherActiveAdministratorExists_ShouldDeactivateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenAnotherActiveAdministratorExists_ShouldDeactivateUser));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var administratorRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.AdministratorRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var targetAdmin = User.CreateWithPassword(
      "Target",
      "Admin",
      "target-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    targetAdmin.AssignRole(administratorRoleId);

    var otherAdmin = User.CreateWithPassword(
      "Other",
      "Admin",
      "other-admin@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    otherAdmin.AssignRole(administratorRoleId);

    await context.Users.AddRangeAsync(targetAdmin, otherAdmin);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new StubMediator(context),
      context,
      new FakeUserAccessor { UserId = otherAdmin.Id });

    var result = await handler.Handle(new DeactivateUserCommand(targetAdmin.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.False((await context.Users.FindAsync([targetAdmin.Id], cancellationToken))!.IsActive);
  }

  [Fact]
  public async Task Handle_WhenUserIsNotAdministrator_ShouldDeactivateUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUserIsNotAdministrator_ShouldDeactivateUser));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var userRoleId = await context.Roles
      .Where(x => x.Name == RoleConstraints.UserRoleName)
      .Select(x => x.Id)
      .SingleAsync(cancellationToken);

    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Regular",
      "User",
      "regular-user@example.com",
      passwordHasher.HashPassword("StrongPass123!")).Value;
    user.AssignRole(userRoleId);

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new DeactivateUserHandler(
      new StubMediator(context),
      context,
      new FakeUserAccessor { UserId = Guid.NewGuid() });

    var result = await handler.Handle(new DeactivateUserCommand(user.Id), cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.False((await context.Users.FindAsync([user.Id], cancellationToken))!.IsActive);
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

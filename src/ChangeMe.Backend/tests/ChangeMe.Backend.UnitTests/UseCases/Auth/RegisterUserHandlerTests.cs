using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class RegisterUserHandlerTests
{
  [Fact]
  public async Task Handle_WhenPublicRegistrationDisabled_ReturnsForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPublicRegistrationDisabled_ReturnsForbidden));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(
      context,
      TestAuthOptions.Create(publicRegistrationEnabled: false));

    var result = await handler.Handle(
      new RegisterUserCommand("A", "B", "user@example.com", "StrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Forbidden, result.Status);
    Assert.Contains(AuthSessionUtils.RegistrationDisabledMessage, result.Errors.First());
  }

  [Fact]
  public async Task Handle_WhenEmailVerificationEnabled_ReturnsRequiresVerificationWithoutSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailVerificationEnabled_ReturnsRequiresVerificationWithoutSession));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(
      context,
      TestAuthOptions.Create(emailVerificationEnabled: true));

    var result = await handler.Handle(
      new RegisterUserCommand("Verify", "Me", "verify@example.com", "StrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Created, result.Status);
    Assert.True(result.Value.RequiresEmailVerification);
    Assert.Null(result.Value.AuthSession);

    var user = context.Users.Single(x => x.Email == "verify@example.com");
    Assert.False(user.EmailVerified);
    Assert.True(user.HasPasswordSet);
    Assert.Equal(1, context.UserAuthTokens.Count(x => x.UserId == user.Id));
  }

  [Fact]
  public async Task Handle_WhenEmailVerificationDisabled_ReturnsAuthSession()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailVerificationDisabled_ReturnsAuthSession));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(context, TestAuthOptions.Create());

    var result = await handler.Handle(
      new RegisterUserCommand("Direct", "Login", "direct@example.com", "StrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Created, result.Status);
    Assert.False(result.Value.RequiresEmailVerification);
    Assert.NotNull(result.Value.AuthSession);
    Assert.False(string.IsNullOrWhiteSpace(result.Value.AuthSession!.Token));

    var user = context.Users.Single(x => x.Email == "direct@example.com");
    Assert.False(user.EmailVerified);
    Assert.Equal(1, context.UserSessions.Count(x => x.UserId == user.Id));
  }

  [Fact]
  public async Task Handle_WhenExistingExternalOnlyAccount_ReturnsConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenExistingExternalOnlyAccount_ReturnsConflict));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var user = User.CreateInvited("external-only@example.com", "Oidc", "User").Value;
    var login = ExternalLogin.Create(user.Id, "google", "subject-1").Value;
    user.AddExternalLogin(login);
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context, TestAuthOptions.Create());

    var result = await handler.Handle(
      new RegisterUserCommand("Attacker", "User", "external-only@example.com", "StrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Conflict, result.Status);
    Assert.Contains(AuthSessionUtils.DuplicateEmailMessage, result.Errors.First());
    Assert.False(context.Users.Single(x => x.Id == user.Id).HasPasswordSet);
  }

  [Fact]
  public async Task Handle_WhenInvitationCanceledAccount_CompletesExistingUser()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenInvitationCanceledAccount_CompletesExistingUser));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var user = User.CreateInvited("canceled@example.com").Value;
    user.RecordInvitationIssued(DateTime.UtcNow);
    user.CancelPendingInvitations(DateTime.UtcNow);
    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = CreateHandler(context, TestAuthOptions.Create());

    var result = await handler.Handle(
      new RegisterUserCommand("New", "Name", "canceled@example.com", "StrongPass123!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Created, result.Status);
    Assert.NotNull(result.Value.AuthSession);

    var updated = context.Users.Single(x => x.Id == user.Id);
    Assert.True(updated.HasPasswordSet);
    Assert.Equal("New", updated.FirstName);
    Assert.Equal("Name", updated.LastName);
  }

  private static RegisterUserHandler CreateHandler(
    ChangeMe.Backend.Infrastructure.Persistence.ApplicationDbContext context,
    Microsoft.Extensions.Options.IOptions<AuthOptions> authOptions)
  {
    var httpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

    return new RegisterUserHandler(
      context,
      new PasswordHasherAdapter(),
      new JwtTokenGenerator(authOptions),
      new SessionLifetimeService(authOptions),
      new PasswordExpirationEvaluator(authOptions),
      new UserEmailVerificationService(
        new UserAuthTokenService(context, authOptions, TimeProvider.System),
        new FakeAuthEmailService()),
      authOptions,
      httpContextAccessor);
  }
}

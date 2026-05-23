using Ardalis.Result;
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
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPublicRegistrationDisabled_ReturnsForbidden));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(
      context,
      TestAuthOptions.Create(publicRegistrationEnabled: false));

    var result = await handler.Handle(
      new RegisterUserCommand("A", "B", "user@example.com", "StrongPass123!"),
      CancellationToken.None);

    Assert.Equal(ResultStatus.Forbidden, result.Status);
    Assert.Contains(AuthSessionUtils.RegistrationDisabledMessage, result.Errors.First());
  }

  [Fact]
  public async Task Handle_WhenEmailVerificationEnabled_ReturnsRequiresVerificationWithoutSession()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailVerificationEnabled_ReturnsRequiresVerificationWithoutSession));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(
      context,
      TestAuthOptions.Create(emailVerificationEnabled: true));

    var result = await handler.Handle(
      new RegisterUserCommand("Verify", "Me", "verify@example.com", "StrongPass123!"),
      CancellationToken.None);

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
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailVerificationDisabled_ReturnsAuthSession));
    await UseCasesTestDb.SeedSystemRolesAsync(context);

    var handler = CreateHandler(context, TestAuthOptions.Create());

    var result = await handler.Handle(
      new RegisterUserCommand("Direct", "Login", "direct@example.com", "StrongPass123!"),
      CancellationToken.None);

    Assert.Equal(ResultStatus.Created, result.Status);
    Assert.False(result.Value.RequiresEmailVerification);
    Assert.NotNull(result.Value.AuthSession);
    Assert.False(string.IsNullOrWhiteSpace(result.Value.AuthSession!.Token));

    var user = context.Users.Single(x => x.Email == "direct@example.com");
    Assert.True(user.EmailVerified);
    Assert.Equal(1, context.UserSessions.Count(x => x.UserId == user.Id));
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

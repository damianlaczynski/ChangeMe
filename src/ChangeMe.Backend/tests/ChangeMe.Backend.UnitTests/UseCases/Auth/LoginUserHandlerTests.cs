using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class LoginUserHandlerTests
{
  [Fact]
  public async Task Handle_WhenEmailVerificationEnabledAndUserUnverified_ReturnsUnauthorized()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailVerificationEnabledAndUserUnverified_ReturnsUnauthorized));

    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "unverified@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;

    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();

    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);
    var handler = new LoginUserHandler(
      context,
      passwordHasher,
      new JwtTokenGenerator(authOptions),
      new SessionLifetimeService(authOptions),
      new PasswordExpirationEvaluator(authOptions),
      new TwoFactorPolicyEvaluator(authOptions),
      new PasskeyPolicyEvaluator(authOptions),
      authOptions,
      new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

    var result = await handler.Handle(
      new LoginUserCommand("unverified@example.com", "StrongPass123!"),
      CancellationToken.None);

    Assert.Equal(ResultStatus.Unauthorized, result.Status);
    Assert.Contains(AuthSessionUtils.EmailNotVerifiedMessage, result.Errors.First());
  }
}

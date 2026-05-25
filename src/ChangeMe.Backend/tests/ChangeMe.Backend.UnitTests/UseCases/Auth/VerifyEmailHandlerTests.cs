using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class VerifyEmailHandlerTests
{
  [Fact]
  public async Task Handle_WhenTokenIsValid_ShouldMarkEmailVerified()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenTokenIsValid_ShouldMarkEmailVerified));
    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);
    var tokenService = new UserAuthTokenService(context, authOptions, TimeProvider.System);
    var passwordHasher = new PasswordHasherAdapter();

    var user = User.CreateWithPassword(
      "Test",
      "User",
      "verify@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var plainToken = (await tokenService.IssueTokenAsync(
      user.Id,
      UserAuthTokenType.EmailVerification,
      cancellationToken)).Value;

    var handler = new VerifyEmailHandler(context, tokenService);
    var result = await handler.Handle(new VerifyEmailCommand(plainToken), cancellationToken);

    Assert.True(result.IsSuccess);
    var updated = await context.Users.FindAsync([user.Id], cancellationToken);
    Assert.NotNull(updated);
    Assert.True(updated!.EmailVerified);
    Assert.NotNull(updated.EmailVerifiedAt);
  }

  [Fact]
  public async Task Handle_WhenTokenIsInvalid_ShouldReturnNotFound()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenTokenIsInvalid_ShouldReturnNotFound));
    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);
    var tokenService = new UserAuthTokenService(context, authOptions, TimeProvider.System);
    var handler = new VerifyEmailHandler(context, tokenService);

    var result = await handler.Handle(new VerifyEmailCommand("invalid-token"), cancellationToken);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains(AuthSessionUtils.InvalidEmailVerificationTokenMessage, result.Errors.First());
  }
}

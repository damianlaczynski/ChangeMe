using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class RequestEmailVerificationHandlerTests
{
  [Fact]
  public async Task Handle_WhenUnverifiedUserExists_ShouldIssueVerificationToken()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenUnverifiedUserExists_ShouldIssueVerificationToken));
    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);
    var passwordHasher = new PasswordHasherAdapter();

    var user = User.CreateWithPassword(
      "Test",
      "User",
      "resend@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new RequestEmailVerificationHandler(
      context,
      new UserEmailVerificationService(
        new UserAuthTokenService(context, authOptions, TimeProvider.System),
        new FakeAuthEmailService()));

    var result = await handler.Handle(
      new RequestEmailVerificationCommand("resend@example.com"),
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Equal(AuthSessionUtils.EmailVerificationResendAckMessage, result.Value.Message);
    Assert.Equal(1, context.UserAuthTokens.Count(x =>
      x.UserId == user.Id && x.Type == UserAuthTokenType.EmailVerification));
  }

  [Fact]
  public async Task Handle_WhenEmailUnknown_ShouldStillReturnAckWithoutToken()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenEmailUnknown_ShouldStillReturnAckWithoutToken));
    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);

    var handler = new RequestEmailVerificationHandler(
      context,
      new UserEmailVerificationService(
        new UserAuthTokenService(context, authOptions, TimeProvider.System),
        new FakeAuthEmailService()));

    var result = await handler.Handle(
      new RequestEmailVerificationCommand("missing@example.com"),
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.Empty(context.UserAuthTokens);
  }
}

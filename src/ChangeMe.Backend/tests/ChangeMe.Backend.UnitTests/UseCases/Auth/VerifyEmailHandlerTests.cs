using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using Microsoft.EntityFrameworkCore;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UnitTests.Support;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class VerifyEmailHandlerTests
{
  [Fact]
  public async Task Handle_WhenTokenIsValid_ShouldMarkEmailVerified()
  {
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

    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();

    var plainToken = (await tokenService.IssueTokenAsync(user.Id, UserAuthTokenType.EmailVerification)).Value;

    var handler = new VerifyEmailHandler(context, tokenService);
    var result = await handler.Handle(new VerifyEmailCommand(plainToken), CancellationToken.None);

    Assert.True(result.IsSuccess);
    var updated = await context.Users.FindAsync(user.Id);
    Assert.NotNull(updated);
    Assert.True(updated!.EmailVerified);
    Assert.NotNull(updated.EmailVerifiedAt);
  }

  [Fact]
  public async Task Handle_WhenTokenIsInvalid_ShouldReturnNotFound()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenTokenIsInvalid_ShouldReturnNotFound));
    var authOptions = TestAuthOptions.Create(emailVerificationEnabled: true);
    var tokenService = new UserAuthTokenService(context, authOptions, TimeProvider.System);
    var handler = new VerifyEmailHandler(context, tokenService);

    var result = await handler.Handle(new VerifyEmailCommand("invalid-token"), CancellationToken.None);

    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Contains(AuthSessionUtils.InvalidEmailVerificationTokenMessage, result.Errors.First());
  }
}

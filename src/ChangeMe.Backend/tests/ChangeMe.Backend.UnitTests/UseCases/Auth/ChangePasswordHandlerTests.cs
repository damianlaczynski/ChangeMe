using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class ChangePasswordHandlerTests
{
  [Fact]
  public async Task Handle_WhenPasswordChanges_ShouldUpdatePasswordHash()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPasswordChanges_ShouldUpdatePasswordHash));
    var passwordHasher = new PasswordHasherAdapter();
    const string currentPassword = "StrongPass123!";
    var user = User.Create(
      "Test",
      "User",
      "change@example.com",
      passwordHasher.HashPassword(currentPassword)).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ChangePasswordHandler(
      context,
      passwordHasher,
      new FakeUserAccessor { UserId = user.Id });

    const string newPassword = "NewStrongPass456!";
    var result = await handler.Handle(
      new ChangePasswordCommand(currentPassword, newPassword),
      cancellationToken);

    Assert.True(result.IsSuccess);
    Assert.True(passwordHasher.VerifyPassword(user.PasswordHash, newPassword));
  }

  [Fact]
  public async Task Handle_WhenCurrentPasswordIsIncorrect_ShouldReturnInvalidResult()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenCurrentPasswordIsIncorrect_ShouldReturnInvalidResult));
    var passwordHasher = new PasswordHasherAdapter();
    const string currentPassword = "StrongPass123!";
    var user = User.Create(
      "Test",
      "User",
      "change@example.com",
      passwordHasher.HashPassword(currentPassword)).Value;

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var handler = new ChangePasswordHandler(
      context,
      passwordHasher,
      new FakeUserAccessor { UserId = user.Id });

    var result = await handler.Handle(
      new ChangePasswordCommand("WrongPass123!", "NewStrongPass456!"),
      cancellationToken);

    Assert.Equal(ResultStatus.Invalid, result.Status);
  }
}

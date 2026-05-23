using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UnitTests.Support;
using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class ChangePasswordHandlerTests
{
  [Fact]
  public async Task Handle_WhenPasswordChanges_ShouldSendPasswordChangedEmail()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPasswordChanges_ShouldSendPasswordChangedEmail));
    var passwordHasher = new PasswordHasherAdapter();
    const string currentPassword = "StrongPass123!";
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "change@example.com",
      passwordHasher.HashPassword(currentPassword)).Value;

    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();

    var emailService = new FakeAuthEmailService();
    var handler = new ChangePasswordHandler(
      context,
      passwordHasher,
      new FakeUserAccessor { UserId = user.Id },
      emailService);

    var result = await handler.Handle(
      new ChangePasswordCommand(currentPassword, "NewStrongPass456!"),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(1, emailService.PasswordChangedEmailsSent);
  }
}

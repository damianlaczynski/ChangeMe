using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Sessions;
using Microsoft.EntityFrameworkCore;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UnitTests.Support;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class RequiredChangePasswordHandlerTests
{
  [Fact]
  public async Task Handle_WhenNewPasswordMatchesCurrent_ShouldReturnInvalid()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenNewPasswordMatchesCurrent_ShouldReturnInvalid));
    var passwordHasher = new PasswordHasherAdapter();
    const string password = "StrongPass123!";
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "expired@example.com",
      passwordHasher.HashPassword(password)).Value;

    var session = CreateSession(user.Id);
    await context.Users.AddAsync(user);
    await context.UserSessions.AddAsync(session);
    await context.SaveChangesAsync();

    var userAccessor = new FakeUserAccessor { UserId = user.Id, SessionId = session.Id };
    var handler = new RequiredChangePasswordHandler(
      context,
      passwordHasher,
      userAccessor,
      new FakeAuthEmailService());

    var result = await handler.Handle(
      new RequiredChangePasswordCommand(password),
      CancellationToken.None);

    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public async Task Handle_WhenPasswordChanges_ShouldRevokeOtherSessionsAndKeepCurrent()
  {
    await using var context = UseCasesTestDb.Create(nameof(Handle_WhenPasswordChanges_ShouldRevokeOtherSessionsAndKeepCurrent));
    var passwordHasher = new PasswordHasherAdapter();
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "expired@example.com",
      passwordHasher.HashPassword("OldStrongPass123!")).Value;

    var currentSession = CreateSession(user.Id);
    var otherSession = CreateSession(user.Id);
    await context.Users.AddAsync(user);
    await context.UserSessions.AddRangeAsync(currentSession, otherSession);
    await context.SaveChangesAsync();

    var userAccessor = new FakeUserAccessor { UserId = user.Id, SessionId = currentSession.Id };
    var emailService = new FakeAuthEmailService();
    var handler = new RequiredChangePasswordHandler(
      context,
      passwordHasher,
      userAccessor,
      emailService);

    var result = await handler.Handle(
      new RequiredChangePasswordCommand("NewStrongPass456!"),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(1, emailService.PasswordChangedEmailsSent);

    var sessions = context.UserSessions.Where(x => x.UserId == user.Id).ToList();
    Assert.Null(sessions.Single(x => x.Id == currentSession.Id).RevokedAt);
    Assert.NotNull(sessions.Single(x => x.Id == otherSession.Id).RevokedAt);

    var updatedUser = await context.Users.FindAsync(user.Id);
    Assert.NotNull(updatedUser!.PasswordLastChangedAt);
  }

  private static UserSession CreateSession(Guid userId)
  {
    var signedInAt = DateTime.UtcNow;
    return UserSession.Create(
      userId,
      false,
      "Test Browser",
      "127.0.0.1",
      RefreshTokenGenerator.HashToken(RefreshTokenGenerator.CreateToken()),
      signedInAt.AddDays(1),
      signedInAt).Value;
  }
}

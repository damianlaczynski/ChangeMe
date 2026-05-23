using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Issues;
using ChangeMe.Backend.UnitTests.Support;

namespace ChangeMe.Backend.UnitTests.UseCases.Issues;

public sealed class GetAssignableUsersHandlerTests
{
  [Fact]
  public async Task Handle_WhenEmailVerificationDisabled_IncludesUnverifiedUsersWithPassword()
  {
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenEmailVerificationDisabled_IncludesUnverifiedUsersWithPassword));

    var passwordHasher = new PasswordHasherAdapter();
    var unverified = User.CreateWithPassword(
      "Unverified",
      "User",
      "unverified@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;

    await context.Users.AddAsync(unverified);
    await context.SaveChangesAsync();

    var handler = new GetAssignableUsersHandler(context, TestAuthOptions.Create());
    var result = await handler.Handle(new GetAssignableUsersQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Contains(result.Value, x => x.Id == unverified.Id);
  }

  [Fact]
  public async Task Handle_WhenEmailVerificationEnabled_ExcludesUnverifiedUsers()
  {
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenEmailVerificationEnabled_ExcludesUnverifiedUsers));

    var passwordHasher = new PasswordHasherAdapter();
    var unverified = User.CreateWithPassword(
      "Unverified",
      "User",
      "unverified@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: false).Value;
    var verified = User.CreateWithPassword(
      "Verified",
      "User",
      "verified@example.com",
      passwordHasher.HashPassword("StrongPass123!"),
      emailVerified: true).Value;

    await context.Users.AddRangeAsync(unverified, verified);
    await context.SaveChangesAsync();

    var handler = new GetAssignableUsersHandler(
      context,
      TestAuthOptions.Create(emailVerificationEnabled: true));
    var result = await handler.Handle(new GetAssignableUsersQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.DoesNotContain(result.Value, x => x.Id == unverified.Id);
    Assert.Contains(result.Value, x => x.Id == verified.Id);
  }
}

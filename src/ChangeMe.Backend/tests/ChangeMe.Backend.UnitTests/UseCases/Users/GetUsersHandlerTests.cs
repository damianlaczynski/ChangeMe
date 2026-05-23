using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Common;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UnitTests.Support;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class GetUsersHandlerTests
{
  [Fact]
  public async Task Handle_WhenEmailVerifiedFilterIsSet_ReturnsOnlyMatchingUsers()
  {
    await using var context = UseCasesTestDb.Create(
      nameof(Handle_WhenEmailVerifiedFilterIsSet_ReturnsOnlyMatchingUsers));

    var passwordHasher = new PasswordHasherAdapter();
    var verified = User.CreateWithPassword(
      "Verified",
      "User",
      "verified@example.com",
      passwordHasher.HashPassword("hash"),
      emailVerified: true).Value;
    var unverified = User.CreateWithPassword(
      "Unverified",
      "User",
      "unverified@example.com",
      passwordHasher.HashPassword("hash"),
      emailVerified: false).Value;

    await context.Users.AddRangeAsync(verified, unverified);
    await context.SaveChangesAsync();

    var handler = new GetUsersHandler(context);
    var result = await handler.Handle(
      new GetUsersQuery
      {
        EmailVerified = [false],
        PaginationParameters = PaginationParameters<UserListItemDto>.Create()
      },
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(result.Value.Items);
    Assert.Equal(unverified.Id, result.Value.Items[0].Id);
    Assert.False(result.Value.Items[0].EmailVerified);
  }
}

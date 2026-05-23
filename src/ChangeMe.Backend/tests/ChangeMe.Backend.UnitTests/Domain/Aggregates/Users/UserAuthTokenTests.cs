using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;

namespace ChangeMe.Backend.UnitTests.Domain.Aggregates.Users;

public sealed class UserAuthTokenTests
{
  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var userId = Guid.CreateVersion7();
    var expiresAt = DateTime.UtcNow.AddHours(24);

    var result = UserAuthToken.Create(
      userId,
      UserAuthTokenType.PasswordReset,
      new string('a', 64),
      expiresAt);

    Assert.True(result.IsSuccess);
    Assert.Equal(userId, result.Value.UserId);
    Assert.Equal(UserAuthTokenType.PasswordReset, result.Value.Type);
    Assert.Equal(expiresAt, result.Value.ExpiresAtUtc);
    Assert.False(result.Value.IsUsed);
  }

  [Fact]
  public void IsValid_WhenUsed_ShouldBeFalse()
  {
    var token = UserAuthToken.Create(
      Guid.CreateVersion7(),
      UserAuthTokenType.Invitation,
      new string('b', 64),
      DateTime.UtcNow.AddHours(1)).Value;

    token.MarkUsed(DateTime.UtcNow);

    Assert.False(token.IsValid(DateTime.UtcNow));
  }

  [Fact]
  public void IsValid_WhenExpired_ShouldBeFalse()
  {
    var token = UserAuthToken.Create(
      Guid.CreateVersion7(),
      UserAuthTokenType.EmailVerification,
      new string('c', 64),
      DateTime.UtcNow.AddHours(-1)).Value;

    Assert.False(token.IsValid(DateTime.UtcNow));
  }
}

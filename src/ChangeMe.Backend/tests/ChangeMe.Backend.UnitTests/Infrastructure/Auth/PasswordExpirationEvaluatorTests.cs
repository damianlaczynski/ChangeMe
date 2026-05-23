using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class PasswordExpirationEvaluatorTests
{
  [Fact]
  public void IsPasswordChangeRequired_WhenDisabled_ReturnsFalse()
  {
    var evaluator = CreateEvaluator(enabled: false, maxAgeDays: 90);
    var user = User.CreateWithPassword("Test", "User", "test@example.com", "hash").Value;

    Assert.False(evaluator.IsPasswordChangeRequired(user, DateTime.UtcNow));
  }

  [Fact]
  public void IsPasswordChangeRequired_WhenWithinAge_ReturnsFalse()
  {
    var evaluator = CreateEvaluator(enabled: true, maxAgeDays: 90);
    var lastChanged = DateTime.UtcNow.AddDays(-30);
    var user = CreateUserWithPasswordLastChangedAt(lastChanged);

    Assert.False(evaluator.IsPasswordChangeRequired(user, DateTime.UtcNow));
  }

  [Fact]
  public void IsPasswordChangeRequired_WhenExpired_ReturnsTrue()
  {
    var evaluator = CreateEvaluator(enabled: true, maxAgeDays: 90);
    var lastChanged = DateTime.UtcNow.AddDays(-91);
    var user = CreateUserWithPasswordLastChangedAt(lastChanged);

    Assert.True(evaluator.IsPasswordChangeRequired(user, DateTime.UtcNow));
  }

  [Fact]
  public void IsPasswordChangeRequired_WhenInvitePending_ReturnsFalse()
  {
    var evaluator = CreateEvaluator(enabled: true, maxAgeDays: 90);
    var user = User.CreateInvited("invite@example.com").Value;

    Assert.False(evaluator.IsPasswordChangeRequired(user, DateTime.UtcNow));
  }

  [Fact]
  public void GetPasswordExpiresAtUtc_WhenEnabled_ReturnsLastChangedPlusMaxAge()
  {
    var evaluator = CreateEvaluator(enabled: true, maxAgeDays: 30);
    var lastChanged = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var user = CreateUserWithPasswordLastChangedAt(lastChanged);

    Assert.Equal(lastChanged.AddDays(30), evaluator.GetPasswordExpiresAtUtc(user));
  }

  private static PasswordExpirationEvaluator CreateEvaluator(bool enabled, int maxAgeDays) =>
    new(Options.Create(new AuthOptions
    {
      PasswordExpirationEnabled = enabled,
      MaximumPasswordAgeDays = maxAgeDays
    }));

  private static User CreateUserWithPasswordLastChangedAt(DateTime passwordLastChangedAt)
  {
    var user = User.CreateWithPassword("Test", "User", "test@example.com", "hash").Value;
    typeof(User).GetProperty(nameof(User.PasswordLastChangedAt))!
      .SetValue(user, passwordLastChangedAt);
    return user;
  }
}

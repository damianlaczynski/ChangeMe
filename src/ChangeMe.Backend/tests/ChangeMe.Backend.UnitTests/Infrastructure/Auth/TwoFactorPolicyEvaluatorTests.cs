using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class TwoFactorPolicyEvaluatorTests
{
  [Fact]
  public void IsTwoFactorSetupRequired_WhenRequiredAndNotEnrolled_ShouldReturnTrue()
  {
    var evaluator = CreateEvaluator(twoFactorEnabled: true, twoFactorRequired: true);
    var user = CreateUser(hasPasswordSet: true, twoFactorEnabled: false);

    Assert.True(evaluator.IsTwoFactorSetupRequired(user));
  }

  [Fact]
  public void IsTwoFactorSetupRequired_WhenInvitePending_ShouldReturnFalse()
  {
    var evaluator = CreateEvaluator(twoFactorEnabled: true, twoFactorRequired: true);
    var user = CreateUser(hasPasswordSet: false, twoFactorEnabled: false);

    Assert.False(evaluator.IsTwoFactorSetupRequired(user));
  }

  [Fact]
  public void IsTwoFactorSetupRequired_WhenAlreadyEnrolled_ShouldReturnFalse()
  {
    var evaluator = CreateEvaluator(twoFactorEnabled: true, twoFactorRequired: true);
    var user = CreateUser(hasPasswordSet: true, twoFactorEnabled: true);

    Assert.False(evaluator.IsTwoFactorSetupRequired(user));
  }

  private static TwoFactorPolicyEvaluator CreateEvaluator(
    bool twoFactorEnabled,
    bool twoFactorRequired) =>
    new(Options.Create(new AuthOptions
    {
      TwoFactor = new TwoFactorOptions
      {
        Enabled = twoFactorEnabled,
        Required = twoFactorRequired
      }
    }));

  private static User CreateUser(bool hasPasswordSet, bool twoFactorEnabled)
  {
    var result = hasPasswordSet
      ? User.CreateWithPassword("Test", "User", "user@example.com", "hash")
      : User.CreateInvited("user@example.com");

    Assert.True(result.IsSuccess);
    var user = result.Value;

    if (twoFactorEnabled)
    {
      var enableResult = user.EnableTwoFactor("protected-secret", DateTime.UtcNow);
      Assert.True(enableResult.IsSuccess);
    }

    return user;
  }
}

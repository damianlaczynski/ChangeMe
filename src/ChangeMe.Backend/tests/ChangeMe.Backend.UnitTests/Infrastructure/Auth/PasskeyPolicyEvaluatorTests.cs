using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UnitTests.Infrastructure.Auth;

public sealed class PasskeyPolicyEvaluatorTests
{
  [Fact]
  public void IsPasskeySetupRequired_WhenRequiredAndNoPasskeys_ShouldReturnTrue()
  {
    var evaluator = CreateEvaluator(passkeysEnabled: true, passkeysRequired: true);
    var user = User.CreateWithPassword("Test", "User", "user@example.com", "hash").Value;

    Assert.True(evaluator.IsPasskeySetupRequired(user, passkeyCount: 0));
  }

  [Fact]
  public void IsPasskeySetupRequired_WhenInvitePending_ShouldReturnFalse()
  {
    var evaluator = CreateEvaluator(passkeysEnabled: true, passkeysRequired: true);
    var utcNow = DateTime.UtcNow;
    var user = User.CreateInvited("invited@example.com").Value;
    user.RecordInvitationIssued(utcNow, utcNow.AddHours(72));

    Assert.True(user.HasPendingInvitation);
    Assert.False(evaluator.IsPasskeySetupRequired(user, passkeyCount: 0));
  }

  [Fact]
  public void IsPasskeySetupRequired_WhenExternalOnlyWithoutInvitation_ShouldReturnTrue()
  {
    var evaluator = CreateEvaluator(passkeysEnabled: true, passkeysRequired: true);
    var user = User.CreateInvited("external-only@example.com").Value;

    Assert.False(user.HasPendingInvitation);
    Assert.True(evaluator.IsPasskeySetupRequired(user, passkeyCount: 0));
  }

  [Fact]
  public void IsPasskeySetupRequired_WhenPasskeyRegistered_ShouldReturnFalse()
  {
    var evaluator = CreateEvaluator(passkeysEnabled: true, passkeysRequired: true);
    var user = User.CreateWithPassword("Test", "User", "user@example.com", "hash").Value;

    Assert.False(evaluator.IsPasskeySetupRequired(user, passkeyCount: 1));
  }

  private static PasskeyPolicyEvaluator CreateEvaluator(
    bool passkeysEnabled,
    bool passkeysRequired) =>
    new(Options.Create(new AuthOptions
    {
      Passkeys = new PasskeyOptions
      {
        PasskeysAuthenticationEnabled = passkeysEnabled,
        PasskeysAuthenticationRequired = passkeysRequired
      }
    }));
}

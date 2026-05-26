using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Auth;

public sealed class PasskeyAuthUtilsTests
{
  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenCeremonyHasNoEmail_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5)).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.True(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenEmailsMatch_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5),
      normalizedEmail: User.NormalizeEmail("user@example.com")).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.True(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void DoesCeremonyEmailMatchUser_WhenEmailsDiffer_ShouldReturnFalse()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.Authentication,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5),
      normalizedEmail: User.NormalizeEmail("other@example.com")).Value;

    var user = User.CreateWithPassword("A", "B", "user@example.com", "hash").Value;

    Assert.False(PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user));
  }

  [Fact]
  public void IsAttemptLimitReached_WhenCountEqualsMax_ShouldReturnTrue()
  {
    var ceremony = WebAuthnCeremonyPending.Create(
      WebAuthnCeremonyType.StepUp,
      """{"challenge":"abc"}""",
      DateTime.UtcNow.AddMinutes(5)).Value;

    for (var i = 0; i < 5; i++)
      ceremony.RecordFailedAttempt();

    Assert.True(PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, 5));
  }
}

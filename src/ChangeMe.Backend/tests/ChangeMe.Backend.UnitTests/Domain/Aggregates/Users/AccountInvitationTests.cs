using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.UnitTests.Domain.Aggregates.Users;

public sealed class AccountInvitationTests
{
  [Fact]
  public void GetExpiresAtUtc_WhenLifetimeHoursProvided_ShouldAddToSentAt()
  {
    var sentAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var invitation = AccountInvitation.Create(Guid.NewGuid(), sentAt).Value;

    Assert.Equal(sentAt.AddHours(72), invitation.GetExpiresAtUtc(72));
  }

  [Fact]
  public void IsPending_WhenAccepted_ShouldBeFalse()
  {
    var utcNow = DateTime.UtcNow;
    var invitation = AccountInvitation.Create(Guid.NewGuid(), utcNow).Value;

    invitation.Accept(utcNow);

    Assert.False(invitation.IsPending);
  }

  [Fact]
  public void IsPending_WhenRevoked_ShouldBeFalse()
  {
    var utcNow = DateTime.UtcNow;
    var invitation = AccountInvitation.Create(Guid.NewGuid(), utcNow).Value;

    invitation.Revoke(utcNow);

    Assert.False(invitation.IsPending);
  }

  [Fact]
  public void IsExpired_WhenPastComputedExpiry_ShouldBeTrue()
  {
    var sentAt = DateTime.UtcNow.AddHours(-80);
    var invitation = AccountInvitation.Create(Guid.NewGuid(), sentAt).Value;

    Assert.True(invitation.IsExpired(DateTime.UtcNow, invitationLinkLifetimeHours: 72));
  }
}

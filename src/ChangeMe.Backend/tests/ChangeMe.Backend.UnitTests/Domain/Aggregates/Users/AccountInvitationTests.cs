using ChangeMe.Backend.Domain.Aggregates.Users.Entities;

namespace ChangeMe.Backend.UnitTests.Domain.Aggregates.Users;

public sealed class AccountInvitationTests
{
  [Fact]
  public void IsPending_WhenNewlyCreated_ShouldBeTrue()
  {
    var invitation = AccountInvitation.Create(
      Guid.NewGuid(),
      DateTime.UtcNow,
      DateTime.UtcNow.AddHours(72)).Value;

    Assert.True(invitation.IsPending);
  }

  [Fact]
  public void IsPending_WhenAccepted_ShouldBeFalse()
  {
    var utcNow = DateTime.UtcNow;
    var invitation = AccountInvitation.Create(Guid.NewGuid(), utcNow, utcNow.AddHours(72)).Value;

    invitation.Accept(utcNow);

    Assert.False(invitation.IsPending);
  }

  [Fact]
  public void IsPending_WhenRevoked_ShouldBeFalse()
  {
    var utcNow = DateTime.UtcNow;
    var invitation = AccountInvitation.Create(Guid.NewGuid(), utcNow, utcNow.AddHours(72)).Value;

    invitation.Revoke(utcNow);

    Assert.False(invitation.IsPending);
  }
}

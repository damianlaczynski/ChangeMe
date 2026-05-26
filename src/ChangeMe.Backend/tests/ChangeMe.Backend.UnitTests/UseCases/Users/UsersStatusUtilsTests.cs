using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UnitTests.UseCases.Users;

public sealed class UsersStatusUtilsTests
{
  [Fact]
  public void ComputeStatus_WhenDeactivated_ShouldReturnDeactivated()
  {
    var status = UsersStatusUtils.ComputeStatus(
      deactivated: true,
      invitationPending: true,
      hasPasswordSet: false,
      hasExternalLogin: false);

    Assert.Equal(UserMembershipStatus.Deactivated, status);
  }

  [Fact]
  public void ComputeStatus_WhenInvitationPending_ShouldReturnInvited()
  {
    var status = UsersStatusUtils.ComputeStatus(
      deactivated: false,
      invitationPending: true,
      hasPasswordSet: false,
      hasExternalLogin: false);

    Assert.Equal(UserMembershipStatus.Invited, status);
  }

  [Fact]
  public void ComputeStatus_WhenNoPasswordAndNoExternalLogin_ShouldReturnInvitationCanceled()
  {
    var status = UsersStatusUtils.ComputeStatus(
      deactivated: false,
      invitationPending: false,
      hasPasswordSet: false,
      hasExternalLogin: false);

    Assert.Equal(UserMembershipStatus.InvitationCanceled, status);
  }

  [Fact]
  public void ComputeStatus_WhenPasswordSet_ShouldReturnActive()
  {
    var user = User.CreateWithPassword(
      "Test",
      "User",
      "active@example.com",
      "hash").Value;

    Assert.Equal(UserMembershipStatus.Active, UsersStatusUtils.ComputeStatus(user));
  }
}

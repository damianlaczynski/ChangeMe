using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;

namespace ChangeMe.Backend.UnitTests;

public sealed class ProjectTests
{
  [Fact]
  public void Create_WhenInputIsValid_ShouldNormalizeKeyAndTrimFields()
  {
    var result = Project.Create("  Platform  ", "  plat  ", "  Workspace  ", ProjectVisibility.INTERNAL, "#112233");

    Assert.True(result.IsSuccess);
    Assert.Equal("Platform", result.Value.Name);
    Assert.Equal("PLAT", result.Value.Key);
    Assert.Equal("Workspace", result.Value.Description);
    Assert.Equal("#112233", result.Value.Color);
    Assert.Equal(ProjectStatus.ACTIVE, result.Value.Status);
  }

  [Fact]
  public void Create_WhenKeyIsInvalid_ShouldReturnInvalidResult()
  {
    var result = Project.Create("Platform", "x", null, ProjectVisibility.INTERNAL);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void AddMember_WhenUserAlreadyMember_ShouldReturnConflict()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var userId = Guid.CreateVersion7();

    Assert.True(project.AddMember(userId, ProjectMemberRole.OWNER).IsSuccess);
    var secondAdd = project.AddMember(userId, ProjectMemberRole.MEMBER);

    Assert.False(secondAdd.IsSuccess);
    Assert.Equal(ResultStatus.Conflict, secondAdd.Status);
  }

  [Fact]
  public void RemoveMember_WhenRemovingLastOwner_ShouldReturnError()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var ownerId = Guid.CreateVersion7();
    project.AddMember(ownerId, ProjectMemberRole.OWNER);

    var result = project.RemoveMember(ownerId);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void UpdateMemberRole_WhenDemotingLastOwner_ShouldReturnError()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var ownerId = Guid.CreateVersion7();
    project.AddMember(ownerId, ProjectMemberRole.OWNER);

    var result = project.UpdateMemberRole(ownerId, ProjectMemberRole.MEMBER);

    Assert.False(result.IsSuccess);
    Assert.Equal(ProjectMemberRole.OWNER, project.Members.Single().Role);
  }

  [Fact]
  public void UpdateMemberRole_WhenAnotherOwnerExists_ShouldUpdateRole()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var firstOwnerId = Guid.CreateVersion7();
    var secondOwnerId = Guid.CreateVersion7();
    project.AddMember(firstOwnerId, ProjectMemberRole.OWNER);
    project.AddMember(secondOwnerId, ProjectMemberRole.OWNER);

    var result = project.UpdateMemberRole(firstOwnerId, ProjectMemberRole.MEMBER);

    Assert.True(result.IsSuccess);
    Assert.Equal(ProjectMemberRole.MEMBER, project.Members.Single(m => m.UserId == firstOwnerId).Role);
  }

  [Fact]
  public void CanManage_WhenUserIsOwner_ShouldReturnTrue()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var ownerId = Guid.CreateVersion7();
    project.AddMember(ownerId, ProjectMemberRole.OWNER);

    Assert.True(project.CanManage(ownerId));
  }

  [Fact]
  public void CanManage_WhenUserIsMember_ShouldReturnFalse()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var memberId = Guid.CreateVersion7();
    project.AddMember(memberId, ProjectMemberRole.MEMBER);

    Assert.False(project.CanManage(memberId));
  }

  [Fact]
  public void IsAccessibleBy_WhenProjectIsInternal_ShouldAllowAnyUser()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.INTERNAL).Value;

    Assert.True(project.IsAccessibleBy(Guid.CreateVersion7()));
  }

  [Fact]
  public void IsAccessibleBy_WhenProjectIsPrivate_ShouldAllowOnlyMembers()
  {
    var project = Project.Create("Platform", "PLAT", null, ProjectVisibility.PRIVATE).Value;
    var memberId = Guid.CreateVersion7();
    project.AddMember(memberId, ProjectMemberRole.MEMBER);

    Assert.True(project.IsAccessibleBy(memberId));
    Assert.False(project.IsAccessibleBy(Guid.CreateVersion7()));
  }
}

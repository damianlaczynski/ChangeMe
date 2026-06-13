using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ProjectMembersEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostProjectMember_WhenAdministratorAddsUser_ShouldReturnUpdatedProject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var administrator = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var member = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Membership project",
      "MEM",
      cancellationToken,
      ownerUserId: administrator.UserId);

    using var client = administrator.Client;
    var response = await client.PostAsJsonAsync(
      $"/api/projects/{projectId}/members",
      new
      {
        UserId = member.UserId,
        Role = ProjectMemberRole.MEMBER
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<ProjectDetailsResponse>>(cancellationToken);
    Assert.NotNull(body);
    Assert.Contains(
      body!.Value.Members,
      m => m.UserId == member.UserId && m.Role == ProjectMemberRole.MEMBER);
  }

  [Fact]
  public async Task PutProjectMemberRole_WhenDemotingLastOwner_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var administrator = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Owner project",
      "OWN",
      cancellationToken,
      ownerUserId: administrator.UserId);

    using var client = administrator.Client;
    var response = await client.PutAsJsonAsync(
      $"/api/projects/{projectId}/members/{administrator.UserId}",
      new { Role = ProjectMemberRole.MEMBER },
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task DeleteProjectMember_WhenRemovingLastOwner_ShouldReturnBadRequest()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var administrator = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Protected owner",
      "PRO",
      cancellationToken,
      ownerUserId: administrator.UserId);

    using var client = administrator.Client;
    var response = await client.DeleteAsync(
      $"/api/projects/{projectId}/members/{administrator.UserId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task DeleteProjectMember_WhenAnotherOwnerExists_ShouldRemoveMember()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var administrator = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var secondOwner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Dual owner",
      "DUA",
      cancellationToken,
      ownerUserId: administrator.UserId);

    using var client = administrator.Client;

    var addResponse = await client.PostAsJsonAsync(
      $"/api/projects/{projectId}/members",
      new
      {
        UserId = secondOwner.UserId,
        Role = ProjectMemberRole.OWNER
      },
      cancellationToken);
    addResponse.EnsureSuccessStatusCode();

    var response = await client.DeleteAsync(
      $"/api/projects/{projectId}/members/{administrator.UserId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<ProjectDetailsResponse>>(cancellationToken);
    Assert.NotNull(body);
    Assert.DoesNotContain(body!.Value.Members, m => m.UserId == administrator.UserId);
    Assert.Contains(body.Value.Members, m => m.UserId == secondOwner.UserId);
  }

  [Fact]
  public async Task PutProject_WhenUserIsNotOwner_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var outsider = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Owned project",
      "OWN2",
      cancellationToken,
      ownerUserId: owner.UserId);

    using var client = outsider.Client;
    var response = await client.PutAsJsonAsync(
      $"/api/projects/{projectId}",
      new
      {
        Id = projectId,
        Name = "Renamed",
        Key = "OWN2",
        Visibility = ProjectVisibility.INTERNAL,
        Status = ProjectStatus.ACTIVE
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task PutProject_WhenUserIsOwner_ShouldUpdateProject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Owned project",
      "OWN3",
      cancellationToken,
      ownerUserId: owner.UserId);

    using var client = owner.Client;
    var response = await client.PutAsJsonAsync(
      $"/api/projects/{projectId}",
      new
      {
        Id = projectId,
        Name = "Renamed project",
        Key = "OWN3",
        Visibility = ProjectVisibility.INTERNAL,
        Status = ProjectStatus.ACTIVE
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  private sealed record ProjectMemberResponse(Guid UserId, ProjectMemberRole Role);

  private sealed record ProjectDetailsResponse(IReadOnlyList<ProjectMemberResponse> Members);

  private sealed record ApiEnvelope<T>(T Value);
}

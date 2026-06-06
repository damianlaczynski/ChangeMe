using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class IssueProjectAccessEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetAllIssues_WhenUserIsNotProjectMember_ShouldNotReturnPrivateProjectIssues()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var outsider = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);
    var marker = $"private-{Guid.NewGuid():N}";

    await IssueTestHelper.SeedIssueAsync(
      factory,
      $"{marker} private issue",
      "Hidden from outsider",
      IssuePriority.MEDIUM,
      null,
      cancellationToken,
      projectId: projectId,
      actorId: owner.UserId);

    var response = await outsider.Client.GetAsync(
      $"/api/issues?pageNumber=1&pageSize=50&searchText={Uri.EscapeDataString(marker)}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.DoesNotContain(marker, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetIssueById_WhenUserIsNotProjectMember_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var outsider = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var issueId = await IssueTestHelper.SeedIssueAsync(
      factory,
      "Private issue",
      "Description",
      IssuePriority.MEDIUM,
      null,
      cancellationToken,
      projectId: projectId,
      actorId: owner.UserId);

    var response = await outsider.Client.GetAsync($"/api/issues/{issueId}", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectPermissionCodes.ForbiddenMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PostIssues_WhenUserIsViewerOnProject_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var viewer = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);
    await ProjectTestHelper.AddMemberAsync(owner.Client, projectId, viewer.UserId, ProjectRole.VIEWER, cancellationToken);

    var response = await viewer.Client.PostAsJsonAsync("/api/issues", new
    {
      ProjectId = projectId,
      Title = "Viewer cannot create",
      Description = "Should fail",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      WatchAfterCreate = false
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task PostIssues_WhenUserHasManageOnDefaultProject_ShouldReturnCreated()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var defaultProjectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/issues", new
    {
      ProjectId = defaultProjectId,
      Title = $"Default project issue {Guid.NewGuid():N}",
      Description = "Created on Default",
      Status = IssueStatus.NEW,
      Priority = IssuePriority.MEDIUM,
      WatchAfterCreate = false
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }

  [Fact]
  public async Task GetAllIssues_WhenFilteredByProjectId_ShouldReturnOnlyMatchingProject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var defaultProjectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);
    var customProjectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);
    var marker = $"filter-{Guid.NewGuid():N}";

    await IssueTestHelper.SeedIssueAsync(
      factory,
      $"{marker} default",
      "On default",
      IssuePriority.LOW,
      null,
      cancellationToken,
      projectId: defaultProjectId,
      actorId: user.UserId);

    await IssueTestHelper.SeedIssueAsync(
      factory,
      $"{marker} custom",
      "On custom",
      IssuePriority.HIGH,
      null,
      cancellationToken,
      projectId: customProjectId,
      actorId: user.UserId);

    var response = await user.Client.GetAsync(
      $"/api/issues?pageNumber=1&pageSize=50&searchText={Uri.EscapeDataString(marker)}&projectId={customProjectId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains($"{marker} custom", body, StringComparison.Ordinal);
    Assert.DoesNotContain($"{marker} default", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetAllIssues_WhenFilteredByUnauthorizedProject_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var outsider = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var response = await outsider.Client.GetAsync(
      $"/api/issues?pageNumber=1&pageSize=10&projectId={projectId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }
}

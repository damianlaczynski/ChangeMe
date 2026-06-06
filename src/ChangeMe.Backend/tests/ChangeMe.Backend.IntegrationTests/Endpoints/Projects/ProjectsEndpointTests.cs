using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;

namespace ChangeMe.Backend.IntegrationTests.Endpoints.Projects;

[Collection(IntegrationTestCollection.Name)]
public sealed class ProjectsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task GetProjects_WhenAuthenticated_ShouldIncludeDefaultProject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync("/api/projects?pageNumber=1&pageSize=10", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectConstraints.DefaultProjectName, body, StringComparison.Ordinal);
    Assert.Contains("MEMBER", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostProjects_WhenAuthenticated_ShouldCreateProjectAndAssignOwner()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectName = $"Custom-{Guid.NewGuid():N}";

    var response = await user.Client.PostAsJsonAsync("/api/projects", new
    {
      Name = projectName,
      Description = "Custom project description"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(projectName, body, StringComparison.Ordinal);
    Assert.Contains("OWNER", body, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("\"canManage\":true", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task PostProjects_WhenDuplicateName_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectName = $"Duplicate-{Guid.NewGuid():N}";
    await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken, projectName);

    var response = await user.Client.PostAsJsonAsync("/api/projects", new
    {
      Name = projectName.ToUpperInvariant(),
      Description = "Another description"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectConstraints.DuplicateNameMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task GetProjectById_WhenMember_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);

    var response = await user.Client.GetAsync($"/api/projects/{projectId}", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task GetProjectById_WhenNotMember_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var outsider = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var response = await outsider.Client.GetAsync($"/api/projects/{projectId}", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectPermissionCodes.ForbiddenMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PutProject_WhenOwnerUpdatesCustomProject_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);
    var updatedName = $"Updated-{Guid.NewGuid():N}";

    var response = await user.Client.PutAsJsonAsync($"/api/projects/{projectId}", new
    {
      Id = projectId,
      Name = updatedName,
      Description = "Updated description"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(updatedName, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task PutProject_WhenSystemProject_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var defaultProjectId = await ProjectTestHelper.GetDefaultProjectIdAsync(factory, cancellationToken);

    var response = await user.Client.PutAsJsonAsync($"/api/projects/{defaultProjectId}", new
    {
      Id = defaultProjectId,
      Name = "Renamed Default",
      Description = "Should fail"
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task DeleteProject_WhenProjectHasIssues_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);

    await IssueTestHelper.SeedIssueAsync(
      factory,
      "Issue blocking delete",
      "Description",
      Domain.Aggregates.Issue.Enums.IssuePriority.MEDIUM,
      null,
      cancellationToken,
      projectId: projectId,
      actorId: user.UserId);

    var response = await user.Client.DeleteAsync($"/api/projects/{projectId}", cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectConstraints.HasIssuesDeleteMessage, body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeleteProject_WhenEmptyCustomProject_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);

    var response = await user.Client.DeleteAsync($"/api/projects/{projectId}", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task PostProjectMember_WhenOwnerAddsMember_ShouldReturnOk()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var member = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var response = await owner.Client.PostAsJsonAsync($"/api/projects/{projectId}/members", new
    {
      ProjectId = projectId,
      UserId = member.UserId,
      Role = Domain.Aggregates.Project.Enums.ProjectRole.MEMBER
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task PostProjectMember_WhenUserAlreadyMember_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var member = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);
    await ProjectTestHelper.AddMemberAsync(
      owner.Client,
      projectId,
      member.UserId,
      Domain.Aggregates.Project.Enums.ProjectRole.MEMBER,
      cancellationToken);

    var response = await owner.Client.PostAsJsonAsync($"/api/projects/{projectId}/members", new
    {
      ProjectId = projectId,
      UserId = member.UserId,
      Role = Domain.Aggregates.Project.Enums.ProjectRole.VIEWER
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  [Fact]
  public async Task PutProjectMemberRole_WhenLastOwnerDemotesSelf_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var response = await owner.Client.PutAsJsonAsync(
      $"/api/projects/{projectId}/members/{owner.UserId}/role",
      new
      {
        ProjectId = projectId,
        UserId = owner.UserId,
        Role = Domain.Aggregates.Project.Enums.ProjectRole.MEMBER
      },
      cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("Assign another owner before changing your own role.", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task DeleteProjectMember_WhenLastOwner_ShouldReturnConflict()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);

    var response = await owner.Client.DeleteAsync(
      $"/api/projects/{projectId}/members/{owner.UserId}",
      cancellationToken);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  [Fact]
  public async Task GetProjectMembershipHistory_WhenMemberAdded_ShouldReturnEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var owner = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var member = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(owner.Client, cancellationToken);
    await ProjectTestHelper.AddMemberAsync(
      owner.Client,
      projectId,
      member.UserId,
      Domain.Aggregates.Project.Enums.ProjectRole.MEMBER,
      cancellationToken);

    var response = await owner.Client.GetAsync(
      $"/api/projects/{projectId}/membership-history?pageNumber=1&pageSize=10",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("MEMBER_ADDED", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetProjectOperationHistory_WhenProjectCreated_ShouldReturnEntry()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    var projectId = await ProjectTestHelper.CreateProjectAsync(user.Client, cancellationToken);

    var response = await user.Client.GetAsync(
      $"/api/projects/{projectId}/operation-history?pageNumber=1&pageSize=10",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains("PROJECT_CREATED", body, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public async Task GetManageableProjects_WhenUserIsDefaultMember_ShouldIncludeDefaultProject()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);

    var response = await user.Client.GetAsync(
      "/api/projects/manageable?permissionCode=Project.Issues.Manage",
      cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(ProjectConstraints.DefaultProjectName, body, StringComparison.Ordinal);
  }
}

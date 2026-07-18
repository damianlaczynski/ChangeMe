using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class IssuePermissionsEndpointTests(BackendWebApplicationFactory factory)
{
  private const string PermissionDeniedMessage = "You do not have permission to perform this action.";

  [Fact]
  public async Task GetIssues_WhenUserLacksIssuesView_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      permissionCodes: [PermissionCodes.SessionsViewOwn, PermissionCodes.SessionsManageOwn]);

    var user = await TestAuthHelper.CreateUserWithRoleAsync(factory, roleId, cancellationToken);

    var response = await user.Client.GetAsync("/api/v1/issues", cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    await AssertPermissionDeniedMessageAsync(response, cancellationToken);
  }

  [Fact]
  public async Task CreateIssue_WhenUserLacksIssuesCreate_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      permissionCodes: [PermissionCodes.IssuesView]);

    var user = await TestAuthHelper.CreateUserWithRoleAsync(factory, roleId, cancellationToken);

    var response = await user.Client.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Blocked issue",
      Description = "Should not be created",
      Status = "NEW",
      Priority = "MEDIUM",
      WatchAfterCreate = false
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    await AssertPermissionDeniedMessageAsync(response, cancellationToken);
  }

  [Fact]
  public async Task UpdateIssue_WhenAuthorHasViewOnly_ShouldAllowTitleChange()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      permissionCodes: [PermissionCodes.IssuesView, PermissionCodes.IssuesCreate]);

    var user = await TestAuthHelper.CreateUserWithRoleAsync(factory, roleId, cancellationToken);

    var createResponse = await user.Client.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Author issue",
      Description = "Original description",
      Status = "NEW",
      Priority = "MEDIUM",
      WatchAfterCreate = false
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();
    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    var issueId = ReadGuidFromResultBody(createBody, "id");
    var version = ReadLongFromResultBody(createBody, "version");

    var updateResponse = await user.Client.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Title = "Author updated title",
      Description = "Original description",
      Status = "NEW",
      Priority = "MEDIUM",
      AssignedToUserId = (Guid?)null,
      Version = version
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
  }

  [Fact]
  public async Task UpdateIssue_WhenAuthorLacksEditAndChangesAssignee_ShouldReturnForbidden()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var admin = await TestAuthHelper.CreateAdministratorUserAsync(factory, cancellationToken);
    var roleId = await RolesTestHelper.CreateCustomRoleAsync(
      admin.Client,
      cancellationToken,
      permissionCodes: [PermissionCodes.IssuesView, PermissionCodes.IssuesCreate]);

    var author = await TestAuthHelper.CreateUserWithRoleAsync(factory, roleId, cancellationToken);
    var assignee = await RolesTestHelper.CreateManagedUserAsync(admin.Client, factory, cancellationToken);

    var createResponse = await author.Client.PostAsJsonAsync("/api/v1/issues", new
    {
      Title = "Assignee blocked",
      Description = "Description",
      Status = "NEW",
      Priority = "MEDIUM",
      WatchAfterCreate = false
    }, cancellationToken);

    createResponse.EnsureSuccessStatusCode();
    var createBody = await createResponse.Content.ReadAsStringAsync(cancellationToken);
    var issueId = ReadGuidFromResultBody(createBody, "id");
    var version = ReadLongFromResultBody(createBody, "version");

    var updateResponse = await author.Client.PutAsJsonAsync($"/api/v1/issues/{issueId}", new
    {
      Id = issueId,
      Title = "Assignee blocked",
      Description = "Description",
      Status = "NEW",
      Priority = "MEDIUM",
      AssignedToUserId = assignee.UserId,
      Version = version
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    await AssertPermissionDeniedMessageAsync(updateResponse, cancellationToken);
  }

  private static async Task AssertPermissionDeniedMessageAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
  {
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    Assert.Contains(PermissionDeniedMessage, body, StringComparison.Ordinal);
  }

  private static Guid ReadGuidFromResultBody(string json, string propertyName) =>
    RolesTestHelper.ReadGuidFromResultBody(json, propertyName);

  private static long ReadLongFromResultBody(string json, string propertyName)
  {
    using var document = JsonDocument.Parse(json);
    var value = document.RootElement.TryGetProperty("value", out var camelValue)
      ? camelValue
      : document.RootElement;

    if (value.TryGetProperty(propertyName, out var camelProperty))
      return camelProperty.GetInt64();

    var pascalName = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];
    return value.GetProperty(pascalName).GetInt64();
  }

  private static string ExtractToken(string responseBody)
  {
    using var document = JsonDocument.Parse(responseBody);
    var value = document.RootElement.GetProperty("value");
    if (value.TryGetProperty("authSession", out var authSession)
        && authSession.TryGetProperty("token", out var nestedToken))
      return nestedToken.GetString() ?? throw new InvalidOperationException("Token is null.");

    return value.GetProperty("token").GetString()
      ?? throw new InvalidOperationException("Token is null.");
  }
}

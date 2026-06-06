using System.Net.Http.Json;
using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class ProjectTestHelper
{
  public static async Task<Guid> GetDefaultProjectIdAsync(
    BackendWebApplicationFactory factory,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Projects
      .AsNoTracking()
      .Where(p => p.NormalizedName == Project.NormalizeName(ProjectConstraints.DefaultProjectName))
      .Select(p => p.Id)
      .SingleAsync(cancellationToken);
  }

  public static async Task<Guid> CreateProjectAsync(
    HttpClient client,
    CancellationToken cancellationToken,
    string? name = null,
    string? description = null)
  {
    var projectName = name ?? $"Project-{Guid.NewGuid():N}";
    var response = await client.PostAsJsonAsync("/api/projects", new
    {
      Name = projectName,
      Description = description ?? "Integration test project"
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    return RolesTestHelper.ReadGuidFromResultBody(body, "id");
  }

  public static async Task AddMemberAsync(
    HttpClient client,
    Guid projectId,
    Guid userId,
    ProjectRole role,
    CancellationToken cancellationToken)
  {
    var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/members", new
    {
      ProjectId = projectId,
      UserId = userId,
      Role = role
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
  }

  public static async Task ChangeMemberRoleAsync(
    HttpClient client,
    Guid projectId,
    Guid userId,
    ProjectRole role,
    CancellationToken cancellationToken)
  {
    var response = await client.PutAsJsonAsync($"/api/projects/{projectId}/members/{userId}/role", new
    {
      ProjectId = projectId,
      UserId = userId,
      Role = role
    }, cancellationToken);

    response.EnsureSuccessStatusCode();
  }

  public static async Task<Guid> GetIssueProjectIdAsync(
    BackendWebApplicationFactory factory,
    Guid issueId,
    CancellationToken cancellationToken)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Issues
      .AsNoTracking()
      .Where(i => i.Id == issueId)
      .Select(i => i.ProjectId)
      .SingleAsync(cancellationToken);
  }

  public static string? ReadErrorMessage(string responseBody) =>
    IntegrationApiJson.ReadErrorMessage(responseBody);
}

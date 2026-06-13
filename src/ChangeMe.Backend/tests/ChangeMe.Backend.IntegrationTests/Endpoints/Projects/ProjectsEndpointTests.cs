using System.Net;
using System.Net.Http.Json;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using ChangeMe.Backend.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ProjectsEndpointTests(BackendWebApplicationFactory factory)
{
  [Fact]
  public async Task PostProjects_WhenUserCanManageProjects_ShouldCreateProjectWithOwnerMember()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    using var client = user.Client;

    var request = new
    {
      Name = "Customer portal",
      Key = "CUST",
      Description = "Customer-facing improvements",
      Visibility = ProjectVisibility.INTERNAL,
      Color = "#2563EB"
    };

    var response = await client.PostAsJsonAsync("/api/projects", request, cancellationToken);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var project = await dbContext.Projects
      .Include(p => p.Members)
      .SingleOrDefaultAsync(p => p.Key == "CUST", cancellationToken);

    Assert.NotNull(project);
    Assert.Equal(request.Name, project.Name);
    Assert.Single(project.Members, m => m.UserId == user.UserId && m.Role == ProjectMemberRole.OWNER);
  }

  [Fact]
  public async Task GetProjectsForSelection_WhenUserIsAuthenticated_ShouldReturnAccessibleProjects()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var user = await TestAuthHelper.CreateAuthenticatedUserAsync(factory, cancellationToken);
    await ProjectTestHelper.SeedProjectAsync(
      factory,
      "Visible project",
      "VIS",
      cancellationToken,
      ownerUserId: user.UserId);
    using var client = user.Client;

    var response = await client.GetAsync("/api/projects/for-selection", cancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<ProjectSelectionItem[]>>(cancellationToken);
    Assert.NotNull(body);
    Assert.Contains(body!.Value, p => p.Key == "VIS");
  }

  [Fact]
  public async Task PostProjects_WhenUserIsAnonymous_ShouldReturnUnauthorized()
  {
    var cancellationToken = TestContext.Current.CancellationToken;

    using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri("https://localhost")
    });

    var response = await client.PostAsJsonAsync("/api/projects", new
    {
      Name = "Unauthorized project",
      Key = "UNA",
      Visibility = ProjectVisibility.INTERNAL
    }, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  private sealed record ProjectSelectionItem(Guid Id, string Name, string Key, string Color, ProjectStatus Status);

  private sealed record ApiEnvelope<T>(T Value);
}

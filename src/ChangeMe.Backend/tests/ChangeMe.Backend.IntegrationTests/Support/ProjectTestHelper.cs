using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class ProjectTestHelper
{
  public static async Task<Guid> SeedProjectAsync(
    BackendWebApplicationFactory factory,
    string name,
    string key,
    CancellationToken cancellationToken,
    ProjectVisibility visibility = ProjectVisibility.INTERNAL,
    Guid? ownerUserId = null)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var effectiveOwnerId = ownerUserId ?? Guid.CreateVersion7();

    var projectResult = Project.Create(name, key, null, visibility);
    var project = projectResult.Value;
    project.AddMember(effectiveOwnerId, ProjectMemberRole.OWNER);

    ApplyAudit(project, effectiveOwnerId);
    ApplyAudit(project.Members, effectiveOwnerId);

    dbContext.Projects.Add(project);
    await dbContext.SaveChangesAsync(cancellationToken);

    return project.Id;
  }

  private static void ApplyAudit(Entity entity, Guid actorId)
  {
    entity.CreatedBy = actorId;
    entity.UpdatedBy = actorId;
  }

  private static void ApplyAudit(IEnumerable<Entity> entities, Guid actorId)
  {
    foreach (var entity in entities)
      ApplyAudit(entity, actorId);
  }
}

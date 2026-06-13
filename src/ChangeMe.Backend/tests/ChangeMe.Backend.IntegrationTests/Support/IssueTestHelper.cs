using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using ChangeMe.Backend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeMe.Backend.IntegrationTests.Support;

internal static class IssueTestHelper
{
  public static async Task<Guid> SeedIssueAsync(
    BackendWebApplicationFactory factory,
    string title,
    string description,
    IssuePriority priority,
    string[]? acceptanceCriteria,
    CancellationToken cancellationToken,
    IssueStatus status = IssueStatus.NEW,
    Guid? assignedToUserId = null,
    Guid? actorId = null,
    bool addActorAsWatcher = false,
    Guid? projectId = null)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var effectiveActorId = actorId ?? Guid.CreateVersion7();
    var effectiveProjectId = projectId ?? await SeedDefaultProjectInScopeAsync(dbContext, effectiveActorId, cancellationToken);

    var issueResult = Issue.Create(effectiveProjectId, title, description, priority, status, assignedToUserId);
    var issue = issueResult.Value;
    issue.RecordCreation(effectiveActorId);

    foreach (var acceptanceCriterion in acceptanceCriteria ?? [])
    {
      issue.AddAcceptanceCriterion(acceptanceCriterion);
    }

    if (addActorAsWatcher)
      issue.StartWatching(effectiveActorId);

    ApplyAudit(issue, effectiveActorId);
    ApplyAudit(issue.HistoryEntries, effectiveActorId);
    ApplyAudit(issue.AcceptanceCriteria, effectiveActorId);
    ApplyAudit(issue.Comments, effectiveActorId);
    ApplyAudit(issue.Watchers, effectiveActorId);

    dbContext.Issues.Add(issue);
    await dbContext.SaveChangesAsync(cancellationToken);

    return issue.Id;
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

  private static async Task<Guid> SeedDefaultProjectInScopeAsync(
    ApplicationDbContext dbContext,
    Guid actorId,
    CancellationToken cancellationToken)
  {
    var existingProjectId = await dbContext.Projects
      .Select(p => p.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (existingProjectId != Guid.Empty)
      return existingProjectId;

    var projectResult = Project.Create("Test project", "TEST", null, ProjectVisibility.INTERNAL);
    var project = projectResult.Value;
    project.AddMember(actorId, ProjectMemberRole.OWNER);
    ApplyAudit(project, actorId);
    ApplyAudit(project.Members, actorId);
    dbContext.Projects.Add(project);
    await dbContext.SaveChangesAsync(cancellationToken);
    return project.Id;
  }
}

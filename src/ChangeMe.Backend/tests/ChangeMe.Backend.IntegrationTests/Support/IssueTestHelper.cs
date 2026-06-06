using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;
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
    Guid? projectId = null,
    bool addActorAsWatcher = false)
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var effectiveActorId = actorId ?? Guid.CreateVersion7();
    var effectiveProjectId = projectId ?? await EnsureDefaultProjectIdAsync(dbContext, effectiveActorId, cancellationToken);

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

  private static async Task<Guid> EnsureDefaultProjectIdAsync(
    ApplicationDbContext dbContext,
    Guid actorId,
    CancellationToken cancellationToken)
  {
    var existing = await dbContext.Projects
      .AsNoTracking()
      .FirstOrDefaultAsync(
        p => p.NormalizedName == Project.NormalizeName(ProjectConstraints.DefaultProjectName),
        cancellationToken);

    if (existing is not null)
      return existing.Id;

    var createResult = Project.CreateDefault(ProjectAuthorization.SystemActorUserId);
    var project = createResult.Value;
    ApplyAudit(project, actorId);
    ApplyAudit(project.Members, actorId);
    ApplyAudit(project.MembershipHistory, actorId);
    ApplyAudit(project.OperationHistory, actorId);

    var memberResult = project.EnsureMember(actorId, ProjectRole.OWNER, actorId);
    if (memberResult.IsSuccess)
    {
      var member = project.Members.First(m => m.UserId == actorId);
      ApplyAudit(member, actorId);
      dbContext.ProjectMembers.Add(member);
    }

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

using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.Infrastructure.Persistence;

public static class ProjectDefaultSeeder
{
  public static async Task EnsureDefaultProjectAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    var defaultProject = await context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(
        p => p.NormalizedName == Project.NormalizeName(ProjectConstraints.DefaultProjectName),
        cancellationToken);

    if (defaultProject is null)
    {
      var createResult = Project.CreateDefault(ProjectAuthorization.SystemActorUserId);
      if (!createResult.IsSuccess)
        return;

      defaultProject = createResult.Value;
      await context.Projects.AddAsync(defaultProject, cancellationToken);
      await context.ProjectMembers.AddRangeAsync(defaultProject.Members, cancellationToken);
      await context.ProjectMembershipHistory.AddRangeAsync(defaultProject.MembershipHistory, cancellationToken);
      await context.ProjectOperationHistory.AddRangeAsync(defaultProject.OperationHistory, cancellationToken);
    }

    var activeUserIds = await context.Users
      .AsNoTracking()
      .Where(u => !u.Deactivated)
      .Select(u => u.Id)
      .ToListAsync(cancellationToken);

    foreach (var userId in activeUserIds)
    {
      if (defaultProject.HasMember(userId))
        continue;

      var historyBefore = defaultProject.MembershipHistory.Count;
      var memberResult = defaultProject.EnsureMember(
        userId,
        ProjectRole.MEMBER,
        ProjectAuthorization.SystemActorUserId,
        useSystemActor: true);
      if (!memberResult.IsSuccess)
        continue;

      var member = defaultProject.Members.First(m => m.UserId == userId);
      await context.ProjectMembers.AddAsync(member, cancellationToken);

      var newHistory = defaultProject.MembershipHistory.Skip(historyBefore).ToList();
      await context.ProjectMembershipHistory.AddRangeAsync(newHistory, cancellationToken);
    }

    var orphanIssues = await context.Issues
      .Where(i => i.ProjectId == Guid.Empty)
      .ToListAsync(cancellationToken);

    foreach (var issue in orphanIssues)
      context.Entry(issue).Property(nameof(Issue.ProjectId)).CurrentValue = defaultProject.Id;
  }
}

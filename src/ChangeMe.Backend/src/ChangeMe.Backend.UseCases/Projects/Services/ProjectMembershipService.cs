using ChangeMe.Backend.Domain.Aggregates.Issue;
using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;

namespace ChangeMe.Backend.UseCases.Projects.Services;

public class ProjectMembershipService(ApplicationDbContext context)
{
  public async Task<Result<Project>> EnsureDefaultProjectAsync(CancellationToken cancellationToken)
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
        return createResult.Map();

      defaultProject = createResult.Value;
      await context.Projects.AddAsync(defaultProject, cancellationToken);
      await StageNewProjectChildrenAsync(defaultProject, cancellationToken);
    }

    await EnsureActiveUsersAreDefaultMembersAsync(defaultProject, cancellationToken);
    await AssignOrphanIssuesToDefaultAsync(defaultProject.Id, cancellationToken);

    return Result.Success(defaultProject);
  }

  public async Task<Result> AddUserToDefaultProjectAsync(Guid userId, CancellationToken cancellationToken)
  {
    var ensureResult = await EnsureDefaultProjectAsync(cancellationToken);
    if (!ensureResult.IsSuccess)
      return ensureResult.Map();

    var defaultProject = ensureResult.Value;
    if (defaultProject.HasMember(userId))
      return Result.Success();

    return await AddMemberToProjectAsync(
      defaultProject,
      userId,
      ProjectRole.MEMBER,
      useSystemActor: true,
      cancellationToken);
  }

  private async Task EnsureActiveUsersAreDefaultMembersAsync(Project defaultProject, CancellationToken cancellationToken)
  {
    var activeUserIds = await context.Users
      .AsNoTracking()
      .Where(u => !u.Deactivated)
      .Select(u => u.Id)
      .ToListAsync(cancellationToken);

    foreach (var userId in activeUserIds)
    {
      if (defaultProject.HasMember(userId))
        continue;

      var addResult = await AddMemberToProjectAsync(
        defaultProject,
        userId,
        ProjectRole.MEMBER,
        useSystemActor: true,
        cancellationToken);
      if (!addResult.IsSuccess)
        continue;
    }
  }

  private async Task<Result> AddMemberToProjectAsync(
    Project project,
    Guid userId,
    ProjectRole role,
    bool useSystemActor,
    CancellationToken cancellationToken)
  {
    var membershipHistoryCountBefore = project.MembershipHistory.Count;
    var memberResult = project.EnsureMember(
      userId,
      role,
      ProjectAuthorization.SystemActorUserId,
      useSystemActor);
    if (!memberResult.IsSuccess)
      return memberResult.Map();

    var member = project.Members.First(m => m.UserId == userId);
    await context.ProjectMembers.AddAsync(member, cancellationToken);

    var newHistoryEntries = project.MembershipHistory
      .Skip(membershipHistoryCountBefore)
      .ToList();
    await context.ProjectMembershipHistory.AddRangeAsync(newHistoryEntries, cancellationToken);

    return Result.Success();
  }

  private async Task AssignOrphanIssuesToDefaultAsync(Guid defaultProjectId, CancellationToken cancellationToken)
  {
    var orphanIssues = await context.Issues
      .Where(i => i.ProjectId == Guid.Empty)
      .ToListAsync(cancellationToken);

    foreach (var issue in orphanIssues)
      context.Entry(issue).Property(nameof(Issue.ProjectId)).CurrentValue = defaultProjectId;
  }

  private async Task StageNewProjectChildrenAsync(Project project, CancellationToken cancellationToken)
  {
    await context.ProjectMembers.AddRangeAsync(project.Members, cancellationToken);
    await context.ProjectMembershipHistory.AddRangeAsync(project.MembershipHistory, cancellationToken);
    await context.ProjectOperationHistory.AddRangeAsync(project.OperationHistory, cancellationToken);
  }
}

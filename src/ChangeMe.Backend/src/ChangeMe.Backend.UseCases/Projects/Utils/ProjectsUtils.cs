using ChangeMe.Backend.Domain.Aggregates.Project;
using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.UseCases.Projects.Utils;

public static class ProjectsUtils
{
  public static IReadOnlyList<ProjectRole> GetRolesWithPermission(string permissionCode) =>
    permissionCode switch
    {
      ProjectPermissionCodes.View or
      ProjectPermissionCodes.MembersView or
      ProjectPermissionCodes.IssuesView or
      ProjectPermissionCodes.TimeView =>
        [ProjectRole.OWNER, ProjectRole.MEMBER, ProjectRole.VIEWER],
      ProjectPermissionCodes.Manage or
      ProjectPermissionCodes.MembersManage or
      ProjectPermissionCodes.TimeManage =>
        [ProjectRole.OWNER],
      ProjectPermissionCodes.IssuesManage or
      ProjectPermissionCodes.TimeLog =>
        [ProjectRole.OWNER, ProjectRole.MEMBER],
      _ => [],
    };

  public static Result RequireProjectPermission(ProjectRole? role, string permissionCode)
  {
    if (!ProjectAuthorization.HasPermission(role, permissionCode))
      return Result.Forbidden(ProjectPermissionCodes.ForbiddenMessage);

    return Result.Success();
  }

  public static async Task<Result<ProjectRole?>> GetMemberRoleAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var role = await context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.ProjectId == projectId && m.UserId == userId)
      .Select(m => (ProjectRole?)m.Role)
      .FirstOrDefaultAsync(cancellationToken);

    return Result.Success(role);
  }

  public static async Task<Result> EnsureUniqueProjectNameAsync(
    ApplicationDbContext context,
    string name,
    Guid? excludeProjectId,
    CancellationToken cancellationToken)
  {
    var normalizedName = Project.NormalizeName(name);
    var exists = await context.Projects
      .AsNoTracking()
      .AnyAsync(
        p => p.NormalizedName == normalizedName && (!excludeProjectId.HasValue || p.Id != excludeProjectId.Value),
        cancellationToken);

    return exists
      ? Result.Conflict(ProjectConstraints.DuplicateNameMessage)
      : Result.Success();
  }

  public static async Task<Result<Domain.Aggregates.Project.Project>> LoadProjectAsync(
    ApplicationDbContext context,
    Guid projectId,
    bool includeMembers,
    CancellationToken cancellationToken)
  {
    IQueryable<Domain.Aggregates.Project.Project> query = context.Projects;

    if (includeMembers)
      query = query.Include(p => p.Members);

    var project = await query.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    return project is null ? Result.NotFound() : Result.Success(project);
  }

  public static ProjectDetailsDto ToDetailsDto(
    Domain.Aggregates.Project.Project project,
    ProjectRole? currentUserRole,
    int issueCount) =>
    new()
    {
      Id = project.Id,
      Name = project.Name,
      Description = project.Description,
      IsSystem = project.IsSystem,
      CurrentUserRole = currentUserRole ?? default,
      IssueCount = issueCount,
      CanManage = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.Manage),
      CanViewMembers = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.MembersView),
      CanManageMembers = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.MembersManage),
      CanViewIssues = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.IssuesView),
      CanManageIssues = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.IssuesManage),
      CanViewLoggedTime = ProjectAuthorization.HasPermission(currentUserRole, ProjectPermissionCodes.TimeView),
    };

  public static string FormatProjectRole(ProjectRole role) =>
    role switch
    {
      ProjectRole.OWNER => "Owner",
      ProjectRole.MEMBER => "Member",
      ProjectRole.VIEWER => "Viewer",
      _ => role.ToString(),
    };

  public static string? ResolveActorName(Guid actorUserId, IReadOnlyDictionary<Guid, string> userLookup) =>
    actorUserId == ProjectAuthorization.SystemActorUserId
      ? "System"
      : userLookup.GetValueOrDefault(actorUserId);
}

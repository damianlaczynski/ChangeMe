using ChangeMe.Backend.Domain.Aggregates.Projects;
using ChangeMe.Backend.Domain.Aggregates.Projects.Enums;
using ChangeMe.Backend.UseCases.Projects.Dtos;

namespace ChangeMe.Backend.UseCases.Projects.Utils;

public static class ProjectsUtils
{
  public const string DuplicateKeyMessage = "A project with this key already exists.";
  public const string ProjectHasIssuesMessage = "Cannot delete a project that still has issues.";
  public const string ProjectNotAccessibleMessage = "You do not have access to this project.";
  public const string ProjectManageForbiddenMessage = "You do not have permission to manage this project.";
  public const string CannotRemoveLastOwnerMessage = ProjectMemberMessages.CannotRemoveLastOwner;
  public const string UserAlreadyMemberMessage = ProjectMemberMessages.UserAlreadyMember;

  public static async Task<bool> IsKeyTakenAsync(
    ApplicationDbContext context,
    string key,
    Guid? excludeProjectId,
    CancellationToken cancellationToken)
  {
    var normalizedKey = Project.NormalizeKey(key);
    var query = context.Projects.AsNoTracking().Where(p => p.Key == normalizedKey);

    if (excludeProjectId.HasValue)
      query = query.Where(p => p.Id != excludeProjectId.Value);

    return await query.AnyAsync(cancellationToken);
  }

  public static async Task<Result<Project>> GetAccessibleProjectAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var project = await context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

    if (project is null)
      return Result<Project>.NotFound();

    if (!project.IsAccessibleBy(userId))
      return Result<Project>.Forbidden(ProjectNotAccessibleMessage);

    return Result.Success(project);
  }

  public static async Task<Result<Project>> GetAccessibleProjectReadOnlyAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var project = await context.Projects
      .AsNoTracking()
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

    if (project is null)
      return Result<Project>.NotFound();

    if (!project.IsAccessibleBy(userId))
      return Result<Project>.Forbidden(ProjectNotAccessibleMessage);

    return Result.Success(project);
  }

  public static Result EnsureCanManageProject(Project project, Guid userId)
  {
    if (!project.IsAccessibleBy(userId))
      return Result.Forbidden(ProjectNotAccessibleMessage);

    if (!project.CanManage(userId))
      return Result.Forbidden(ProjectManageForbiddenMessage);

    return Result.Success();
  }

  public static IQueryable<Project> ApplyVisibilityFilter(
    IQueryable<Project> query,
    Guid userId) =>
    query.Where(p =>
      p.Visibility == ProjectVisibility.INTERNAL
      || p.Members.Any(m => m.UserId == userId));

  public static ProjectDetailsDto ToDetailsDto(
    Project project,
    int issueCount,
    int memberCount,
    IReadOnlyDictionary<Guid, string> userLookup,
    Guid? currentUserId = null)
  {
    return new ProjectDetailsDto
    {
      Id = project.Id,
      Name = project.Name,
      Key = project.Key,
      Description = project.Description,
      Status = project.Status,
      Visibility = project.Visibility,
      Color = project.Color,
      IssueCount = issueCount,
      MemberCount = memberCount,
      CreatedAt = project.CreatedAt,
      UpdatedAt = project.UpdatedAt,
      CurrentUserRole = currentUserId is Guid userId ? project.GetMemberRole(userId) : null,
      Members = project.Members
        .OrderByDescending(m => m.Role == ProjectMemberRole.OWNER)
        .ThenBy(m => m.CreatedAt)
        .Select(m => new ProjectMemberDto
        {
          UserId = m.UserId,
          DisplayLabel = userLookup.GetValueOrDefault(m.UserId, m.UserId.ToString()),
          Role = m.Role,
          JoinedAt = m.CreatedAt
        })
        .ToList()
    };
  }

  public static async Task<IReadOnlyDictionary<Guid, string>> GetUserDisplayNameLookupAsync(
    ApplicationDbContext context,
    IEnumerable<Guid> userIds,
    CancellationToken cancellationToken)
  {
    var distinctIds = userIds.Distinct().ToList();
    if (distinctIds.Count == 0)
      return new Dictionary<Guid, string>();

    return await context.Users
      .AsNoTracking()
      .Where(u => distinctIds.Contains(u.Id))
      .ToDictionaryAsync(u => u.Id, u => u.DisplayLabel, cancellationToken);
  }
}

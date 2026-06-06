using ChangeMe.Backend.Domain.Aggregates.Project.Enums;
using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Aggregates.Time.Entities;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Time.Utils;

public static class TimeUtils
{
  public const string DateRangeInvalidMessage = "Date from must be on or before Date to.";

  public static Result RequireGlobalPermission(IUserAccessor userAccessor, string permissionCode)
  {
    if (!userAccessor.HasPermission(permissionCode))
      return Result.Forbidden(UsersUtils.PermissionDeniedMessage);

    return Result.Success();
  }

  public static DateOnly TodayUtc() => DateOnly.FromDateTime(DateTime.UtcNow);

  public static Result ValidateDateRange(DateOnly? dateFrom, DateOnly? dateTo)
  {
    if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
      return Result.Invalid(new ValidationError(nameof(dateFrom), DateRangeInvalidMessage));

    return Result.Success();
  }

  public static async Task<int> GetBackdatingLimitDaysAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken)
  {
    var settings = await context.TimeTrackingSettings
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.Id == TimeTrackingSettings.SingletonId, cancellationToken);

    return settings?.BackdatingLimitDays ?? TimeConstraints.DefaultBackdatingLimitDays;
  }

  public static Result ValidateWorkDate(
    DateOnly workDate,
    int backdatingLimitDays,
    bool canLogPastLimit)
  {
    var today = TodayUtc();
    if (workDate > today)
    {
      return Result.Invalid(
        new ValidationError(nameof(workDate), TimeConstraints.WorkDateOutsideRangeMessage));
    }

    if (!canLogPastLimit)
    {
      var earliestAllowed = today.AddDays(-backdatingLimitDays);
      if (workDate < earliestAllowed)
      {
        return Result.Invalid(
          new ValidationError(nameof(workDate), TimeConstraints.WorkDateOutsideRangeMessage));
      }
    }

    return Result.Success();
  }

  public static async Task<Result> ValidateIssueBelongsToProjectAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid? issueId,
    CancellationToken cancellationToken)
  {
    if (!issueId.HasValue)
      return Result.Success();

    var issue = await context.Issues
      .AsNoTracking()
      .Where(i => i.Id == issueId.Value)
      .Select(i => new { i.ProjectId })
      .FirstOrDefaultAsync(cancellationToken);

    if (issue is null)
    {
      return Result.Invalid(
        new ValidationError(nameof(issueId), TimeConstraints.IssueMustBelongToProjectMessage));
    }

    if (issue.ProjectId != projectId)
    {
      return Result.Invalid(
        new ValidationError(nameof(issueId), TimeConstraints.IssueMustBelongToProjectMessage));
    }

    return Result.Success();
  }

  public static async Task<Result<(string ProjectName, string? IssueTitle)>> ResolveProjectAndIssueNamesAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid? issueId,
    CancellationToken cancellationToken)
  {
    var projectName = await context.Projects
      .AsNoTracking()
      .Where(p => p.Id == projectId)
      .Select(p => p.Name)
      .FirstOrDefaultAsync(cancellationToken);

    if (projectName is null)
      return Result.NotFound();

    string? issueTitle = null;
    if (issueId.HasValue)
    {
      issueTitle = await context.Issues
        .AsNoTracking()
        .Where(i => i.Id == issueId.Value && i.ProjectId == projectId)
        .Select(i => i.Title)
        .FirstOrDefaultAsync(cancellationToken);
    }

    return Result.Success((projectName, issueTitle));
  }

  public static bool CanManageOwnEntry(IUserAccessor userAccessor, ProjectRole? projectRole) =>
    userAccessor.HasPermission(PermissionCodes.TimeManageOwn)
    && ProjectAuthorization.HasPermission(projectRole, ProjectPermissionCodes.TimeLog);

  public static bool CanManageAnyEntry(IUserAccessor userAccessor, ProjectRole? projectRole) =>
    userAccessor.HasPermission(PermissionCodes.TimeManageAny)
    || ProjectAuthorization.HasPermission(projectRole, ProjectPermissionCodes.TimeManage);

  public static bool CanManageEntry(
    IUserAccessor userAccessor,
    Guid actorUserId,
    Guid authorUserId,
    ProjectRole? projectRole)
  {
    if (authorUserId == actorUserId)
      return CanManageOwnEntry(userAccessor, projectRole);

    return CanManageAnyEntry(userAccessor, projectRole);
  }

  public static async Task<Result> EnsureCanLogOnProjectAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, projectId, userId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    if (roleResult.Value is null)
      return Result.Forbidden(ProjectPermissionCodes.ForbiddenMessage);

    return ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.TimeLog);
  }

  public static async Task<Result> EnsureCanViewProjectTimeAsync(
    ApplicationDbContext context,
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, projectId, userId, cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    if (roleResult.Value is null)
      return Result.Forbidden(ProjectPermissionCodes.ForbiddenMessage);

    return ProjectsUtils.RequireProjectPermission(roleResult.Value, ProjectPermissionCodes.TimeView);
  }

  public static string FormatDuration(int durationMinutes)
  {
    if (durationMinutes <= 0)
      return "0m";

    var hours = durationMinutes / 60;
    var minutes = durationMinutes % 60;

    if (hours == 0)
      return $"{minutes}m";

    return minutes == 0 ? $"{hours}h" : $"{hours}h {minutes}m";
  }

  public static TimeEntryDto ToDto(
    TimeEntry entry,
    string projectName,
    string? issueTitle,
    string? authorName,
    bool canEdit,
    bool canDelete) =>
    new()
    {
      Id = entry.Id,
      AuthorUserId = entry.AuthorUserId,
      AuthorName = authorName,
      ProjectId = entry.ProjectId,
      ProjectName = projectName,
      IssueId = entry.IssueId,
      IssueTitle = issueTitle,
      WorkDate = entry.WorkDate,
      DurationMinutes = entry.DurationMinutes,
      DurationFormatted = FormatDuration(entry.DurationMinutes),
      Description = entry.Description,
      CreatedAt = entry.CreatedAt,
      UpdatedAt = entry.UpdatedAt,
      CanEdit = canEdit,
      CanDelete = canDelete,
    };

  public static async Task DiscardRunningTimerAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var timer = await context.UserRunningTimers
      .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

    if (timer is not null)
      context.UserRunningTimers.Remove(timer);
  }

  public static async Task<HashSet<Guid>> GetViewableProjectIdsAsync(
    ApplicationDbContext context,
    Guid userId,
    CancellationToken cancellationToken)
  {
    var memberships = await context.ProjectMembers
      .AsNoTracking()
      .Where(m => m.UserId == userId)
      .Select(m => new { m.ProjectId, m.Role })
      .ToListAsync(cancellationToken);

    return memberships
      .Where(m =>
        ProjectAuthorization.HasPermission(m.Role, ProjectPermissionCodes.TimeView)
        || ProjectAuthorization.HasPermission(m.Role, ProjectPermissionCodes.TimeLog))
      .Select(m => m.ProjectId)
      .ToHashSet();
  }
}

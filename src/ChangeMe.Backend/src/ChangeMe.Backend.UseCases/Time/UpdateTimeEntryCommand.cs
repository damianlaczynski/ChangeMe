using ChangeMe.Backend.Domain.Aggregates.Time.Entities;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record UpdateTimeEntryCommand(
  Guid Id,
  Guid ProjectId,
  Guid? IssueId,
  DateOnly WorkDate,
  int DurationMinutes,
  string? Description) : ICommand<TimeEntryDto>;

public class UpdateTimeEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateTimeEntryCommand, TimeEntryDto>
{
  public async Task<Result<TimeEntryDto>> Handle(UpdateTimeEntryCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var entry = await context.TimeEntries.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
    if (entry is null)
      return Result.NotFound();

    var entryRoleResult = await ProjectsUtils.GetMemberRoleAsync(
      context,
      entry.ProjectId,
      actorUserId,
      cancellationToken);
    if (!entryRoleResult.IsSuccess)
      return entryRoleResult.Map();

    if (!TimeUtils.CanManageEntry(userAccessor, actorUserId, entry.AuthorUserId, entryRoleResult.Value))
      return Result.Forbidden(UsersUtils.PermissionDeniedMessage);

    var targetProjectAccess = await TimeUtils.EnsureCanLogOnProjectAsync(
      context,
      command.ProjectId,
      actorUserId,
      cancellationToken);
    if (!targetProjectAccess.IsSuccess)
      return targetProjectAccess.Map();

    var issueValidation = await TimeUtils.ValidateIssueBelongsToProjectAsync(
      context,
      command.ProjectId,
      command.IssueId,
      cancellationToken);
    if (!issueValidation.IsSuccess)
      return issueValidation.Map();

    var backdatingLimit = await TimeUtils.GetBackdatingLimitDaysAsync(context, cancellationToken);
    var workDateValidation = TimeUtils.ValidateWorkDate(
      command.WorkDate,
      backdatingLimit,
      userAccessor.HasPermission(PermissionCodes.TimeLogPastLimit));
    if (!workDateValidation.IsSuccess)
      return workDateValidation.Map();

    var previousWorkDate = entry.WorkDate;
    var previousDurationMinutes = entry.DurationMinutes;
    var previousDescription = entry.Description;
    var previousProjectId = entry.ProjectId;
    var previousIssueId = entry.IssueId;

    var previousNamesResult = await TimeUtils.ResolveProjectAndIssueNamesAsync(
      context,
      previousProjectId,
      previousIssueId,
      cancellationToken);
    if (!previousNamesResult.IsSuccess)
      return previousNamesResult.Map();

    var updateResult = entry.Update(
      command.ProjectId,
      command.IssueId,
      command.WorkDate,
      command.DurationMinutes,
      command.Description);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    var namesResult = await TimeUtils.ResolveProjectAndIssueNamesAsync(
      context,
      entry.ProjectId,
      entry.IssueId,
      cancellationToken);
    if (!namesResult.IsSuccess)
      return namesResult.Map();

    var auditEntry = TimeEntryAuditLogEntry.ForUpdate(
      entry.Id,
      actorUserId,
      entry,
      namesResult.Value.ProjectName,
      namesResult.Value.IssueTitle,
      previousWorkDate,
      previousDurationMinutes,
      previousDescription,
      previousProjectId,
      previousNamesResult.Value.ProjectName,
      previousIssueId,
      previousNamesResult.Value.IssueTitle);
    await context.TimeEntryAuditLog.AddAsync(auditEntry, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    var authorName = await context.Users
      .AsNoTracking()
      .Where(u => u.Id == entry.AuthorUserId)
      .Select(u => u.DisplayLabel)
      .FirstOrDefaultAsync(cancellationToken);

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, entry.ProjectId, actorUserId, cancellationToken);
    var canManage = TimeUtils.CanManageEntry(userAccessor, actorUserId, entry.AuthorUserId, roleResult.Value);

    return Result.Success(TimeUtils.ToDto(
      entry,
      namesResult.Value.ProjectName,
      namesResult.Value.IssueTitle,
      authorName,
      canManage,
      canManage));
  }
}

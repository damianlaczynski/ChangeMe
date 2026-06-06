using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.Domain.Aggregates.Time.Entities;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Dtos;
using ChangeMe.Backend.UseCases.Time.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record CreateTimeEntryCommand(
  Guid ProjectId,
  Guid? IssueId,
  DateOnly WorkDate,
  int DurationMinutes,
  string? Description) : ICommand<TimeEntryDto>;

public class CreateTimeEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateTimeEntryCommand, TimeEntryDto>
{
  public async Task<Result<TimeEntryDto>> Handle(CreateTimeEntryCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var permissionResult = TimeUtils.RequireGlobalPermission(userAccessor, PermissionCodes.TimeLogOwn);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var projectAccessResult = await TimeUtils.EnsureCanLogOnProjectAsync(
      context,
      command.ProjectId,
      actorUserId,
      cancellationToken);
    if (!projectAccessResult.IsSuccess)
      return projectAccessResult.Map();

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

    var entryResult = TimeEntry.Create(
      actorUserId,
      command.ProjectId,
      command.IssueId,
      command.WorkDate,
      command.DurationMinutes,
      command.Description);
    if (!entryResult.IsSuccess)
      return entryResult.Map();

    var namesResult = await TimeUtils.ResolveProjectAndIssueNamesAsync(
      context,
      command.ProjectId,
      command.IssueId,
      cancellationToken);
    if (!namesResult.IsSuccess)
      return namesResult.Map();

    var entry = entryResult.Value;
    await context.TimeEntries.AddAsync(entry, cancellationToken);

    var auditEntry = TimeEntryAuditLogEntry.ForCreate(
      entry.Id,
      actorUserId,
      entry.AuthorUserId,
      entry.ProjectId,
      namesResult.Value.ProjectName,
      entry.IssueId,
      namesResult.Value.IssueTitle,
      entry.WorkDate,
      entry.DurationMinutes,
      entry.Description);
    await context.TimeEntryAuditLog.AddAsync(auditEntry, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(context, entry.ProjectId, actorUserId, cancellationToken);
    var canManage = TimeUtils.CanManageOwnEntry(userAccessor, roleResult.Value);

    return Result.Created(
      TimeUtils.ToDto(
        entry,
        namesResult.Value.ProjectName,
        namesResult.Value.IssueTitle,
        authorName: null,
        canManage,
        canManage),
      $"/time/entries/{entry.Id}");
  }
}

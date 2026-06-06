using ChangeMe.Backend.Domain.Aggregates.Time.Entities;
using ChangeMe.Backend.UseCases.Projects.Utils;
using ChangeMe.Backend.UseCases.Time.Utils;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Time;

public record DeleteTimeEntryCommand(Guid Id) : ICommand<Guid>;

public class DeleteTimeEntryHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeleteTimeEntryCommand, Guid>
{
  public async Task<Result<Guid>> Handle(DeleteTimeEntryCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var entry = await context.TimeEntries.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);
    if (entry is null)
      return Result.NotFound();

    var roleResult = await ProjectsUtils.GetMemberRoleAsync(
      context,
      entry.ProjectId,
      actorUserId,
      cancellationToken);
    if (!roleResult.IsSuccess)
      return roleResult.Map();

    if (!TimeUtils.CanManageEntry(userAccessor, actorUserId, entry.AuthorUserId, roleResult.Value))
      return Result.Forbidden(UsersUtils.PermissionDeniedMessage);

    var namesResult = await TimeUtils.ResolveProjectAndIssueNamesAsync(
      context,
      entry.ProjectId,
      entry.IssueId,
      cancellationToken);
    if (!namesResult.IsSuccess)
      return namesResult.Map();

    var auditEntry = TimeEntryAuditLogEntry.ForDelete(
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

    context.TimeEntries.Remove(entry);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(entry.Id);
  }
}

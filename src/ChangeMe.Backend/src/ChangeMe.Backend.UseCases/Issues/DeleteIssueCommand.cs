using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record DeleteIssueCommand(
    Guid Id) : ICommand<Guid>;

public class DeleteIssueHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IFileStorageService fileStorageService) : ICommandHandler<DeleteIssueCommand, Guid>
{
  public async Task<Result<Guid>> Handle(DeleteIssueCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result.Unauthorized();

    var issue = await context.Issues
      .Include(i => i.Attachments)
      .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

    if (issue is null)
      return Result.NotFound();

    var accessResult = await IssuesUtils.ValidateProjectIssueAccessAsync(
      context,
      issue.ProjectId,
      actorUserId,
      ProjectPermissionCodes.IssuesManage,
      cancellationToken);
    if (!accessResult.IsSuccess)
      return accessResult.Map();

    var storageKeysByContainer = issue.Attachments
      .GroupBy(a => a.StorageContainer)
      .ToList();

    context.Issues.Remove(issue);
    await context.SaveChangesAsync(cancellationToken);

    foreach (var group in storageKeysByContainer)
    {
      var storageKeys = group.Select(a => a.StorageKey).ToList();
      if (storageKeys.Count > 0)
        await fileStorageService.DeleteManyAsync(group.Key, issue.Id, storageKeys, cancellationToken);
    }

    return Result.Success(issue.Id);
  }
}

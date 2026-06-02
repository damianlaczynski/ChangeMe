using ChangeMe.Backend.Domain.Aggregates.Issue.Entities;
using ChangeMe.Backend.Infrastructure.FileStorage;
using ChangeMe.Backend.UseCases.Issues.Dtos;
using ChangeMe.Backend.UseCases.Issues.Services;
using ChangeMe.Backend.UseCases.Issues.Utils;

namespace ChangeMe.Backend.UseCases.Issues;

public record UploadIssueAttachmentCommand(
  Guid IssueId,
  string OriginalFileName,
  string? DeclaredContentType,
  byte[] Content) : ICommand<IssueAttachmentDto>;

public class UploadIssueAttachmentHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  AttachmentUploadCoordinator attachmentUploadCoordinator,
  IssueNotificationService issueNotificationService) : ICommandHandler<UploadIssueAttachmentCommand, IssueAttachmentDto>
{
  public async Task<Result<IssueAttachmentDto>> Handle(
    UploadIssueAttachmentCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid actorUserId)
      return Result<IssueAttachmentDto>.Unauthorized();

    var validationResult = attachmentUploadCoordinator.ValidateUpload(
      command.OriginalFileName,
      command.DeclaredContentType,
      command.Content,
      command.Content.LongLength);

    if (!validationResult.IsSuccess)
      return validationResult.Map();

    var issue = await context.Issues
      .Include(i => i.Attachments)
      .Include(i => i.HistoryEntries)
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == command.IssueId, cancellationToken);

    if (issue is null)
      return Result<IssueAttachmentDto>.NotFound();

    var pendingResult = issue.AddPendingAttachment(
      validationResult.Value.SanitizedFileName,
      validationResult.Value.ContentType,
      command.Content.LongLength);

    if (!pendingResult.IsSuccess)
      return pendingResult.Map();

    var pendingAttachment = pendingResult.Value;

    var reserveResult = await attachmentUploadCoordinator.ReservePendingAsync(pendingAttachment, cancellationToken);
    if (!reserveResult.IsSuccess)
      return reserveResult.Map();

    await using var contentStream = new MemoryStream(command.Content, writable: false);
    var storageResult = await attachmentUploadCoordinator.WriteContentAsync(
      pendingAttachment,
      contentStream,
      cancellationToken);

    if (!storageResult.IsSuccess)
    {
      await attachmentUploadCoordinator.RollbackPendingAsync(pendingAttachment, cancellationToken);
      return storageResult.Map();
    }

    var historyCountBeforeActivation = issue.HistoryEntries.Count;

    var activateResult = issue.ActivateAttachment(pendingAttachment.Id, actorUserId);
    if (!activateResult.IsSuccess)
    {
      await attachmentUploadCoordinator.RollbackPendingAsync(pendingAttachment, cancellationToken);
      return activateResult.Map();
    }

    var newHistoryEntries = issue.HistoryEntries
      .Skip(historyCountBeforeActivation)
      .ToList();

    if (newHistoryEntries.Count > 0)
      await context.IssueHistoryEntries.AddRangeAsync(newHistoryEntries, cancellationToken);

    try
    {
      await context.SaveChangesAsync(cancellationToken);
    }
    catch
    {
      await attachmentUploadCoordinator.RollbackPendingAsync(pendingAttachment, cancellationToken);
      throw;
    }

    foreach (var historyEntryId in newHistoryEntries
               .Where(h => IssuesUtils.IsNotificationEligible(h.EventType))
               .Select(h => h.Id))
      await issueNotificationService.NotifyIssueActivityAsync(issue.Id, historyEntryId, actorUserId, cancellationToken);

    var userLookup = await IssuesUtils.GetUserDisplayNameLookupAsync(
      context,
      [actorUserId],
      cancellationToken);

    return Result.Created(new IssueAttachmentDto
    {
      Id = pendingAttachment.Id,
      OriginalFileName = pendingAttachment.OriginalFileName,
      ContentType = pendingAttachment.ContentType,
      SizeBytes = pendingAttachment.SizeBytes,
      UploadedByUserId = actorUserId,
      UploadedByName = userLookup.GetValueOrDefault(actorUserId),
      CreatedAt = pendingAttachment.CreatedAt,
      CanDelete = true
    }, $"/issues/{issue.Id}/attachments/{pendingAttachment.Id}");
  }
}

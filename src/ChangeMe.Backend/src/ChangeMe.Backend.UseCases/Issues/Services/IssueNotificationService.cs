using ChangeMe.Backend.Domain.Aggregates.Issue.Enums;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;

namespace ChangeMe.Backend.UseCases.Issues.Services;

public class IssueNotificationService(
  ApplicationDbContext context,
  INotificationRealtimePublisher realtimePublisher,
  IEmailService emailService)
{
  public async Task NotifyIssueActivityAsync(
    Guid issueId,
    Guid issueHistoryEntryId,
    Guid actorUserId,
    CancellationToken cancellationToken)
  {
    var issue = await context.Issues
      .AsNoTracking()
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);

    var historyEntry = await context.IssueHistoryEntries
      .AsNoTracking()
      .FirstOrDefaultAsync(h => h.Id == issueHistoryEntryId, cancellationToken);

    if (issue is null || historyEntry is null)
      return;

    var recipientIds = issue.Watchers
      .Select(w => w.UserId)
      .Where(userId => userId != actorUserId)
      .Distinct()
      .ToList();

    if (recipientIds.Count == 0)
      return;

    var recipients = await context.Users
      .AsNoTracking()
      .Where(u => recipientIds.Contains(u.Id))
      .ToDictionaryAsync(u => u.Id, cancellationToken);

    foreach (var recipientId in recipientIds)
    {
      if (!recipients.TryGetValue(recipientId, out var recipient))
        continue;

      var existingNotification = await context.Notifications
        .AsNoTracking()
        .AnyAsync(n => n.RecipientUserId == recipientId && n.IssueHistoryEntryId == issueHistoryEntryId, cancellationToken);

      if (existingNotification)
        continue;

      var notificationType = MapNotificationEventType(historyEntry);
      var message = BuildNotificationMessage(issue.Title, historyEntry);

      var notificationResult = Notification.Create(
        recipientId,
        issue.Id,
        historyEntry.Id,
        notificationType,
        issue.Title,
        message,
        historyEntry.CreatedAt,
        $"/issues/{issue.Id}");

      if (!notificationResult.IsSuccess)
        continue;

      context.Notifications.Add(notificationResult.Value);
      await context.SaveChangesAsync(cancellationToken);

      await realtimePublisher.PublishAsync(
        recipientId,
        new NotificationRealtimeMessage
        {
          NotificationId = notificationResult.Value.Id,
          IssueId = issue.Id,
          EventType = notificationType,
          IssueTitle = issue.Title,
          Message = message,
          OccurredAt = historyEntry.CreatedAt,
          Link = $"/issues/{issue.Id}",
        },
        cancellationToken);

      await emailService.SendEmailAsync(
        recipient.Email,
        $"Issue update: {issue.Title}",
        $"{message}{Environment.NewLine}{Environment.NewLine}Open issue: /issues/{issue.Id}");

      notificationResult.Value.MarkEmailSent();
      await context.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task NotifyCommentAddedAsync(
    Guid issueId,
    Guid commentId,
    Guid actorUserId,
    CancellationToken cancellationToken)
  {
    var issue = await context.Issues
      .AsNoTracking()
      .Include(i => i.Watchers)
      .FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);

    var comment = await context.IssueComments
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

    if (issue is null || comment is null)
      return;

    var recipientIds = issue.Watchers
      .Select(w => w.UserId)
      .Where(userId => userId != actorUserId)
      .Distinct()
      .ToList();

    if (recipientIds.Count == 0)
      return;

    var recipients = await context.Users
      .AsNoTracking()
      .Where(u => recipientIds.Contains(u.Id))
      .ToDictionaryAsync(u => u.Id, cancellationToken);

    foreach (var recipientId in recipientIds)
    {
      if (!recipients.TryGetValue(recipientId, out var recipient))
        continue;

      var existingNotification = await context.Notifications
        .AsNoTracking()
        .AnyAsync(n => n.RecipientUserId == recipientId && n.IssueHistoryEntryId == commentId, cancellationToken);

      if (existingNotification)
        continue;

      var message = $"New comment added in issue '{issue.Title}'.";
      var notificationResult = Notification.Create(
        recipientId,
        issue.Id,
        commentId,
        NotificationEventType.COMMENT_CREATED,
        issue.Title,
        message,
        comment.CreatedAt,
        $"/issues/{issue.Id}");

      if (!notificationResult.IsSuccess)
        continue;

      context.Notifications.Add(notificationResult.Value);
      await context.SaveChangesAsync(cancellationToken);

      await realtimePublisher.PublishAsync(
        recipientId,
        new NotificationRealtimeMessage
        {
          NotificationId = notificationResult.Value.Id,
          IssueId = issue.Id,
          EventType = NotificationEventType.COMMENT_CREATED,
          IssueTitle = issue.Title,
          Message = message,
          OccurredAt = comment.CreatedAt,
          Link = $"/issues/{issue.Id}",
        },
        cancellationToken);

      await emailService.SendEmailAsync(
        recipient.Email,
        $"Issue update: {issue.Title}",
        $"{message}{Environment.NewLine}{Environment.NewLine}Open issue: /issues/{issue.Id}");

      notificationResult.Value.MarkEmailSent();
      await context.SaveChangesAsync(cancellationToken);
    }
  }

  private static NotificationEventType MapNotificationEventType(Domain.Aggregates.Issue.Entities.IssueHistoryEntry historyEntry)
  {
    return historyEntry.EventType switch
    {
      IssueHistoryEventType.STATUS_CHANGED when historyEntry.CurrentValue == IssueStatus.CLOSED.ToString() => NotificationEventType.ISSUE_CLOSED,
      IssueHistoryEventType.STATUS_CHANGED when historyEntry.PreviousValue == IssueStatus.CLOSED.ToString() && historyEntry.CurrentValue != IssueStatus.CLOSED.ToString() => NotificationEventType.ISSUE_REOPENED,
      IssueHistoryEventType.STATUS_CHANGED => NotificationEventType.STATUS_CHANGED,
      IssueHistoryEventType.PRIORITY_CHANGED => NotificationEventType.PRIORITY_CHANGED,
      IssueHistoryEventType.ASSIGNEE_CHANGED => NotificationEventType.ASSIGNEE_CHANGED,
      IssueHistoryEventType.TITLE_CHANGED => NotificationEventType.TITLE_CHANGED,
      IssueHistoryEventType.DESCRIPTION_CHANGED => NotificationEventType.DESCRIPTION_CHANGED,
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED => NotificationEventType.ACCEPTANCE_CRITERION_ADDED,
      IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED => NotificationEventType.ACCEPTANCE_CRITERION_UPDATED,
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED => NotificationEventType.ACCEPTANCE_CRITERION_REMOVED,
      _ => NotificationEventType.STATUS_CHANGED,
    };
  }

  private static string BuildNotificationMessage(string issueTitle, Domain.Aggregates.Issue.Entities.IssueHistoryEntry historyEntry)
  {
    return historyEntry.EventType switch
    {
      IssueHistoryEventType.STATUS_CHANGED => $"Issue '{issueTitle}' status changed from '{historyEntry.PreviousValue}' to '{historyEntry.CurrentValue}'.",
      IssueHistoryEventType.PRIORITY_CHANGED => $"Issue '{issueTitle}' priority changed from '{historyEntry.PreviousValue}' to '{historyEntry.CurrentValue}'.",
      IssueHistoryEventType.ASSIGNEE_CHANGED => $"Issue '{issueTitle}' assignee changed.",
      IssueHistoryEventType.TITLE_CHANGED => $"Issue title changed from '{historyEntry.PreviousValue}' to '{historyEntry.CurrentValue}'.",
      IssueHistoryEventType.DESCRIPTION_CHANGED => $"Issue '{issueTitle}' description was updated.",
      IssueHistoryEventType.ACCEPTANCE_CRITERION_ADDED => $"Issue '{issueTitle}' acceptance criterion added.",
      IssueHistoryEventType.ACCEPTANCE_CRITERION_UPDATED => $"Issue '{issueTitle}' acceptance criterion updated.",
      IssueHistoryEventType.ACCEPTANCE_CRITERION_REMOVED => $"Issue '{issueTitle}' acceptance criterion removed.",
      _ => historyEntry.Summary,
    };
  }
}

using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Aggregates.Notifications;
using ChangeMe.Backend.Domain.Aggregates.Notifications.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.UseCases.Billing.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Billing.Services;

public class AvailabilityNotificationService(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  INotificationRealtimePublisher realtimePublisher,
  IEmailService emailService,
  IOptions<AuthOptions> authOptions,
  ILogger<AvailabilityNotificationService> logger)
{
  public async Task NotifyManualEntryChangedAsync(
    AvailabilityEntry entry,
    Guid actorUserId,
    CancellationToken cancellationToken)
  {
    if (entry.Source != AvailabilityEntrySource.Manual)
      return;

    var actor = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
    if (actor is null)
      return;

    var recipient = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == entry.UserId, cancellationToken);
    if (recipient is null || recipient.Deactivated)
      return;

    var isAdminAction = actorUserId != entry.UserId
                        && userAccessor.HasPermission(PermissionCodes.BillingManageAvailability);
    if (!isAdminAction && actorUserId != entry.UserId)
      return;

    var dateLabel = AvailabilityNotificationUtils.FormatDateRange(entry.StartDate, entry.EndDate);
    var link = AvailabilityNotificationUtils.BuildMyAvailabilityLink(entry.StartDate, entry.EndDate);
    var revisionAt = entry.UpdatedAt ?? entry.CreatedAt;

    if (isAdminAction)
    {
      var message =
        $"Your availability was updated by {actor.DisplayLabel} for {dateLabel}.";
      await CreateNotificationAsync(
        recipient.Id,
        NotificationEventType.AVAILABILITY_UPDATED_BY_ADMIN,
        "Your availability was updated",
        message,
        link,
        entry.Id,
        revisionAt,
        sendEmail: true,
        emailSubject: "Your availability was updated",
        emailBody: BuildAdminAvailabilityEmailBody(
          actor.DisplayLabel,
          dateLabel,
          entry),
        cancellationToken);
      return;
    }

    if (actorUserId != entry.UserId)
      return;

    var selfMessage = $"Your availability was updated for {dateLabel}.";
    await CreateNotificationAsync(
      recipient.Id,
      NotificationEventType.AVAILABILITY_UPDATED_BY_SELF,
      "Your availability was updated",
      selfMessage,
      link,
      entry.Id,
      revisionAt,
      sendEmail: false,
      emailSubject: null,
      emailBody: null,
      cancellationToken);
  }

  public async Task NotifyManualEntryDeletedAsync(
    Guid entryUserId,
    Guid entryId,
    DateOnly startDate,
    DateOnly endDate,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus status,
    DateTime sourceRevisionAt,
    Guid actorUserId,
    CancellationToken cancellationToken)
  {
    var actor = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
    if (actor is null)
      return;

    var recipient = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == entryUserId, cancellationToken);
    if (recipient is null || recipient.Deactivated)
      return;

    var isAdminAction = actorUserId != entryUserId
                        && userAccessor.HasPermission(PermissionCodes.BillingManageAvailability);
    var dateLabel = AvailabilityNotificationUtils.FormatDateRange(startDate, endDate);
    var link = AvailabilityNotificationUtils.BuildMyAvailabilityLink(startDate, endDate);

    if (isAdminAction)
    {
      var message =
        $"Your availability was updated by {actor.DisplayLabel} for {dateLabel}.";
      await CreateNotificationAsync(
        recipient.Id,
        NotificationEventType.AVAILABILITY_UPDATED_BY_ADMIN,
        "Your availability was updated",
        message,
        link,
        entryId,
        sourceRevisionAt,
        sendEmail: true,
        emailSubject: "Your availability was updated",
        emailBody: BuildAdminAvailabilityEmailBody(
          actor.DisplayLabel,
          dateLabel,
          allDay,
          startTime,
          endTime,
          status),
        cancellationToken);
      return;
    }

    if (actorUserId != entryUserId)
      return;

    var selfMessage = $"Your availability was updated for {dateLabel}.";
    await CreateNotificationAsync(
      recipient.Id,
      NotificationEventType.AVAILABILITY_UPDATED_BY_SELF,
      "Your availability was updated",
      selfMessage,
      link,
      entryId,
      sourceRevisionAt,
      sendEmail: false,
      emailSubject: null,
      emailBody: null,
      cancellationToken);
  }

  public async Task NotifyWeeklyPatternChangedAsync(
    WeeklyRecurringPattern pattern,
    Guid actorUserId,
    CancellationToken cancellationToken)
  {
    var actor = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);
    if (actor is null)
      return;

    var recipient = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == pattern.UserId, cancellationToken);
    if (recipient is null || recipient.Deactivated)
      return;

    var isAdminAction = actorUserId != pattern.UserId
                        && userAccessor.HasPermission(PermissionCodes.BillingManageAvailability);
    var link = AvailabilityNotificationUtils.BuildMyAvailabilityLink(null, null);
    var revisionAt = pattern.UpdatedAt ?? pattern.CreatedAt;

    if (isAdminAction)
    {
      var message =
        $"Your weekly availability pattern was updated by {actor.DisplayLabel}.";
      await CreateNotificationAsync(
        recipient.Id,
        NotificationEventType.WEEKLY_PATTERN_UPDATED_BY_ADMIN,
        "Your weekly availability pattern was updated",
        message,
        link,
        pattern.Id,
        revisionAt,
        sendEmail: true,
        emailSubject: "Your weekly availability pattern was updated",
        emailBody: BuildAdminPatternEmailBody(actor.DisplayLabel),
        cancellationToken);
      return;
    }

    if (actorUserId != pattern.UserId)
      return;

    await CreateNotificationAsync(
      recipient.Id,
      NotificationEventType.AVAILABILITY_UPDATED_BY_SELF,
      "Your availability was updated",
      "Your availability was updated for your weekly pattern.",
      link,
      pattern.Id,
      revisionAt,
      sendEmail: false,
      emailSubject: null,
      emailBody: null,
      cancellationToken);
  }

  private async Task CreateNotificationAsync(
    Guid recipientUserId,
    NotificationEventType eventType,
    string title,
    string message,
    string link,
    Guid billingSourceEntityId,
    DateTime billingSourceRevisionAt,
    bool sendEmail,
    string? emailSubject,
    string? emailBody,
    CancellationToken cancellationToken)
  {
    var duplicateExists = await context.Notifications
      .AsNoTracking()
      .AnyAsync(
        n => n.RecipientUserId == recipientUserId
             && n.BillingSourceEntityId == billingSourceEntityId
             && n.BillingSourceRevisionAt == billingSourceRevisionAt
             && n.EventType == eventType,
        cancellationToken);
    if (duplicateExists)
      return;

    var notificationResult = Notification.CreateBilling(
      recipientUserId,
      eventType,
      title,
      message,
      link,
      billingSourceEntityId,
      billingSourceRevisionAt);
    if (!notificationResult.IsSuccess)
      return;

    context.Notifications.Add(notificationResult.Value);
    await context.SaveChangesAsync(cancellationToken);

    await realtimePublisher.PublishAsync(
      recipientUserId,
      new NotificationRealtimeMessage
      {
        NotificationId = notificationResult.Value.Id,
        IssueId = null,
        EventType = eventType,
        IssueTitle = title,
        Message = message,
        CreatedAt = notificationResult.Value.CreatedAt,
        Link = link,
      },
      cancellationToken);

    if (!sendEmail || string.IsNullOrWhiteSpace(emailSubject) || string.IsNullOrWhiteSpace(emailBody))
      return;

    var recipient = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == recipientUserId, cancellationToken);
    if (recipient is null || recipient.Deactivated)
      return;

    try
    {
      await emailService.SendEmailAsync(recipient.Email, emailSubject, emailBody);
      notificationResult.Value.MarkEmailSent();
      await context.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger.LogWarning(
        ex,
        "Availability notification was saved for user {UserId} but email delivery failed.",
        recipientUserId);
    }
  }

  private string BuildAdminAvailabilityEmailBody(
    string actorFullName,
    string dateLabel,
    AvailabilityEntry entry) =>
    BuildAdminAvailabilityEmailBody(
      actorFullName,
      dateLabel,
      entry.AllDay,
      entry.StartTime,
      entry.EndTime,
      entry.Status);

  private string BuildAdminAvailabilityEmailBody(
    string actorFullName,
    string dateLabel,
    bool allDay,
    TimeOnly? startTime,
    TimeOnly? endTime,
    AvailabilityStatus status)
  {
    var availabilityUrl = $"{authOptions.Value.FrontendBaseUrl.TrimEnd('/')}{AvailabilityNotificationUtils.BuildMyAvailabilityLink(null, null)}";
    var timeRange = AvailabilityNotificationUtils.FormatTimeRange(allDay, startTime, endTime);
    var detail =
      $"{actorFullName} updated your availability for {dateLabel}. Status: {status}. Time: {timeRange}.";

    return BrandedEmailTemplates.BuildNotificationEmail(
      "Your availability was updated",
      detail,
      availabilityUrl,
      "View my availability");
  }

  private string BuildAdminPatternEmailBody(string actorFullName)
  {
    var availabilityUrl = $"{authOptions.Value.FrontendBaseUrl.TrimEnd('/')}{AvailabilityNotificationUtils.BuildMyAvailabilityLink(null, null)}";
    var detail = $"{actorFullName} updated your weekly availability pattern.";
    return BrandedEmailTemplates.BuildNotificationEmail(
      "Your weekly availability pattern was updated",
      detail,
      availabilityUrl,
      "View my availability");
  }
}

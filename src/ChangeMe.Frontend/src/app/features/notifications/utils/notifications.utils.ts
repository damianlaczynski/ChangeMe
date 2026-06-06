import { NotificationEventType } from '@features/notifications/models/notification.model';

export function isBillingNotification(eventType: NotificationEventType): boolean {
  return (
    eventType === NotificationEventType.AVAILABILITY_UPDATED_BY_ADMIN ||
    eventType === NotificationEventType.WEEKLY_PATTERN_UPDATED_BY_ADMIN ||
    eventType === NotificationEventType.AVAILABILITY_UPDATED_BY_SELF
  );
}

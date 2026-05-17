export interface NotificationDto {
  id: string;
  issueId: string;
  eventType: NotificationEventType;
  issueTitle: string;
  message: string;
  link: string;
  occurredAt: string;
  isRead: boolean;
  readAt?: string | null;
}

export interface NotificationListDto {
  unreadCount: number;
  items: NotificationDto[];
}

export interface NotificationRealtimeMessage {
  notificationId: string;
  issueId: string;
  eventType: NotificationEventType;
  issueTitle: string;
  message: string;
  link: string;
  occurredAt: string;
}

export interface IssueRealtimeMessage {
  issueId: string;
  eventType: string;
  occurredAt: string;
}

export enum NotificationEventType {
  COMMENT_CREATED = 'COMMENT_CREATED',
  STATUS_CHANGED = 'STATUS_CHANGED',
  PRIORITY_CHANGED = 'PRIORITY_CHANGED',
  ASSIGNEE_CHANGED = 'ASSIGNEE_CHANGED',
  TITLE_CHANGED = 'TITLE_CHANGED',
  ISSUE_REOPENED = 'ISSUE_REOPENED',
  ISSUE_CLOSED = 'ISSUE_CLOSED',
  DESCRIPTION_CHANGED = 'DESCRIPTION_CHANGED',
  ACCEPTANCE_CRITERION_ADDED = 'ACCEPTANCE_CRITERION_ADDED',
  ACCEPTANCE_CRITERION_UPDATED = 'ACCEPTANCE_CRITERION_UPDATED',
  ACCEPTANCE_CRITERION_REMOVED = 'ACCEPTANCE_CRITERION_REMOVED'
}

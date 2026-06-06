import { signal } from '@angular/core';
import { IssueStatus } from '@features/issues/models/issue.model';
import { TimeEntryAuditOperation, TimeReportGroupingMode } from '@features/time/models/time.model';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { destructiveMenuItemDangerClasses } from '@shared/ui/utils/confirmation-dialog.utils';

export const ProjectTimePermissionCodes = {
  timeLog: 'Project.Time.Log',
  timeView: 'Project.Time.View',
  timeManage: 'Project.Time.Manage'
} as const;

export const TimeConstraints = {
  DESCRIPTION_MAX_LENGTH: 500,
  MIN_DURATION_MINUTES: 1,
  MAX_DURATION_MINUTES: 1440,
  MAX_DURATION_HOURS: 24,
  MIN_BACKDATING_LIMIT_DAYS: 0,
  MAX_BACKDATING_LIMIT_DAYS: 3650,
  MY_TIME_PAGE_SIZE: 20,
  ISSUE_TIME_PAGE_SIZE: 10,
  AUDIT_LOG_PAGE_SIZE: 20,
  REPORT_PERSON_ENTRIES_PAGE_SIZE: 20
} as const;

export const TimeMessages = {
  timeLogged: 'Time logged.',
  timeEntrySaved: 'Time entry saved.',
  timeEntryDeleted: 'Time entry deleted.',
  timeSettingsSaved: 'Time settings saved.',
  projectRequired: 'Project is required.',
  workDateOutsideRange: 'Work date is outside the allowed range.',
  durationRange: 'Duration must be between 1 minute and 24 hours.',
  descriptionTooLong: 'Description cannot exceed 500 characters.',
  dateRangeInvalid: 'Date from must be on or before Date to.',
  backdatingLimitInvalid: 'Enter a whole number of days from 0 to 3650.',
  noLoggableProjects: 'You are not a member of any project where you can log time.',
  timerMinimumDuration: 'Timer must run at least 1 minute before logging.',
  timerReplaceConfirm: 'You already have a running timer. Stop it and start a new one?',
  timerDiscardConfirm: 'Discard running timer? Elapsed time will not be saved.',
  deleteTimeEntryConfirm: 'Delete this time entry? This action cannot be undone.',
  noTimeEntries: 'No time entries yet',
  noTimeEntriesFiltered: 'No time entries match the selected filters.',
  noAuditRecords: 'No audit records match the selected filters.',
  noReportResults: 'No time entries match the selected filters.',
  noIssueTimeLogged: 'No time logged on this issue yet',
  noIssueContributors: 'No time logged yet',
  emptyDescription: '—',
  noProjectSelected: 'No project selected',
  runningTimer: 'Running timer',
  timerRunning: 'Timer running',
  viewTimer: 'View timer',
  totalInPeriod: 'Total in period',
  totalLoggedTime: 'Total logged time',
  timeOnIssue: 'Time on this issue',
  contributors: 'Contributors',
  moreContributors: (count: number) => `+${count} more`,
  viewTimeReport: 'View time report'
} as const;

export type TimeDatePresetId = 'this-week' | 'this-month' | 'last-month' | 'last-30-days';

export type TimeLabeledOption<T> = { value: T; label: string };

export type DurationPreset = {
  id: string;
  label: string;
  minutes: number;
};

export const openIssueStatuses: IssueStatus[] = [
  IssueStatus.NEW,
  IssueStatus.IN_PROGRESS,
  IssueStatus.RESOLVED
];

export const durationPresets: DurationPreset[] = [
  { id: '15m', label: '15m', minutes: 15 },
  { id: '30m', label: '30m', minutes: 30 },
  { id: '1h', label: '1h', minutes: 60 },
  { id: '2h', label: '2h', minutes: 120 },
  { id: '4h', label: '4h', minutes: 240 },
  { id: '8h', label: '8h', minutes: 480 }
];

export const dateFilterPresets: TimeLabeledOption<TimeDatePresetId>[] = [
  { value: 'this-week', label: 'This week' },
  { value: 'this-month', label: 'This month' },
  { value: 'last-month', label: 'Last month' }
];

export const reportDateFilterPresets: TimeLabeledOption<TimeDatePresetId>[] = [
  { value: 'this-week', label: 'This week' },
  { value: 'this-month', label: 'This month' },
  { value: 'last-month', label: 'Last month' },
  { value: 'last-30-days', label: 'Last 30 days' }
];

export const timeReportGroupingModes = signal<TimeLabeledOption<TimeReportGroupingMode>[]>([
  { value: TimeReportGroupingMode.ByPerson, label: 'By person' },
  { value: TimeReportGroupingMode.ByProject, label: 'By project' },
  { value: TimeReportGroupingMode.ByIssue, label: 'By issue' },
  { value: TimeReportGroupingMode.PersonAndProject, label: 'Person and project' },
  { value: TimeReportGroupingMode.Overall, label: 'Overall' }
]);

export const auditOperationOptions = signal<
  TimeLabeledOption<TimeEntryAuditOperation>[]
>([
  { value: TimeEntryAuditOperation.CREATED, label: 'Created' },
  { value: TimeEntryAuditOperation.UPDATED, label: 'Updated' },
  { value: TimeEntryAuditOperation.DELETED, label: 'Deleted' }
]);

export const timeDeleteMenuItemDangerClasses = destructiveMenuItemDangerClasses;

export const NO_ISSUE_OPTION_ID = '__no_issue__';

export function formatDuration(minutes: number): string {
  if (minutes <= 0) {
    return '0m';
  }

  if (minutes < 60) {
    return `${minutes}m`;
  }

  const hours = Math.floor(minutes / 60);
  const remainder = minutes % 60;

  if (remainder === 0) {
    return `${hours}h`;
  }

  return `${hours}h ${remainder}m`;
}

export function splitDurationMinutes(totalMinutes: number): { hours: number; minutes: number } {
  const safeMinutes = Math.max(0, Math.floor(totalMinutes));
  return {
    hours: Math.floor(safeMinutes / 60),
    minutes: safeMinutes % 60
  };
}

export function combineDurationMinutes(hours: number, minutes: number): number {
  return Math.max(0, Math.floor(hours)) * 60 + Math.max(0, Math.floor(minutes));
}

export function truncateText(text: string, maxLength: number): string {
  const normalized = text.trim();
  if (normalized.length <= maxLength) {
    return normalized;
  }

  return `${normalized.slice(0, maxLength)}…`;
}

export function formatDescriptionCounter(length: number): string {
  return `${length}/${TimeConstraints.DESCRIPTION_MAX_LENGTH}`;
}

export function toIsoDateString(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function parseIsoDateString(value: string): Date {
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

export function getCurrentMonthDateRange(): { dateFrom: Date; dateTo: Date } {
  const today = startOfDay(new Date());
  return {
    dateFrom: new Date(today.getFullYear(), today.getMonth(), 1),
    dateTo: new Date(today.getFullYear(), today.getMonth() + 1, 0)
  };
}

export function getDateRangeForPreset(preset: TimeDatePresetId): { dateFrom: Date; dateTo: Date } {
  const today = startOfDay(new Date());

  switch (preset) {
    case 'this-week': {
      const day = today.getDay();
      const diffToMonday = day === 0 ? -6 : 1 - day;
      const dateFrom = addDays(today, diffToMonday);
      return { dateFrom, dateTo: today };
    }
    case 'this-month':
      return getCurrentMonthDateRange();
    case 'last-month': {
      const dateFrom = new Date(today.getFullYear(), today.getMonth() - 1, 1);
      const dateTo = new Date(today.getFullYear(), today.getMonth(), 0);
      return { dateFrom, dateTo };
    }
    case 'last-30-days':
      return { dateFrom: addDays(today, -29), dateTo: today };
  }
}

export function getTimeTabLabel(totalMinutes: number, totalFormatted?: string): string {
  if (totalMinutes <= 0) {
    return 'Time';
  }

  return `Time (${totalFormatted ?? formatDuration(totalMinutes)})`;
}

export function getAuditOperationLabel(operation: TimeEntryAuditOperation): string {
  switch (operation) {
    case TimeEntryAuditOperation.CREATED:
      return 'Created';
    case TimeEntryAuditOperation.UPDATED:
      return 'Updated';
    case TimeEntryAuditOperation.DELETED:
      return 'Deleted';
    default:
      return String(operation);
  }
}

export type TimeBadgeSeverity = 'secondary' | 'success' | 'info' | 'warn' | 'danger';

export function getAuditOperationSeverity(
  operation: TimeEntryAuditOperation
): TimeBadgeSeverity {
  switch (operation) {
    case TimeEntryAuditOperation.CREATED:
      return 'success';
    case TimeEntryAuditOperation.UPDATED:
      return 'info';
    case TimeEntryAuditOperation.DELETED:
      return 'danger';
    default:
      return 'secondary';
  }
}

export function getRunningTimerTooltip(timer: {
  projectName?: string | null;
  issueTitle?: string | null;
}): string {
  if (timer.issueTitle && timer.projectName) {
    return `Timer for ${timer.issueTitle} (${timer.projectName})`;
  }

  if (timer.projectName) {
    return `Timer for ${timer.projectName}`;
  }

  return TimeMessages.runningTimer;
}

export function canLogTime(authHasLogOwn: boolean): boolean {
  return authHasLogOwn;
}

export function canViewOwnTime(authHasViewOwn: boolean): boolean {
  return authHasViewOwn;
}

export function canViewTimeReports(authHasViewReports: boolean): boolean {
  return authHasViewReports;
}

export function canManageTimeSettings(authHasRolesManage: boolean): boolean {
  return authHasRolesManage;
}

export const TimePermissionCodes = {
  viewOwn: PermissionCodes.timeViewOwn,
  logOwn: PermissionCodes.timeLogOwn,
  manageOwn: PermissionCodes.timeManageOwn,
  viewReports: PermissionCodes.timeViewReports,
  logPastLimit: PermissionCodes.timeLogPastLimit
} as const;

function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function addDays(date: Date, days: number): Date {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return startOfDay(next);
}

export function formatReportCsvFileName(date: Date = new Date()): string {
  return `time-report-${toIsoDateString(date)}.csv`;
}

export function buildExportReportQuery(params: {
  dateFrom: string;
  dateTo: string;
  projectIds?: string[];
  userIds?: string[];
  groupingMode: TimeReportGroupingMode;
}): Record<string, unknown> {
  return {
    dateFrom: params.dateFrom,
    dateTo: params.dateTo,
    projectIds: params.projectIds,
    userIds: params.userIds,
    groupingMode: params.groupingMode
  };
}

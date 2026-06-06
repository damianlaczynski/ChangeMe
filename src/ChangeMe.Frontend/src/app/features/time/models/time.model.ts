import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';
import { PaginationResult } from '@shared/data/models/pagination-result.model';

export enum TimeReportGroupingMode {
  ByPerson = 'ByPerson',
  ByProject = 'ByProject',
  ByIssue = 'ByIssue',
  PersonAndProject = 'PersonAndProject',
  Overall = 'Overall'
}

export enum TimeEntryAuditOperation {
  CREATED = 'CREATED',
  UPDATED = 'UPDATED',
  DELETED = 'DELETED'
}

export interface TimeEntryDto {
  id: string;
  authorUserId: string;
  authorName?: string | null;
  projectId: string;
  projectName: string;
  issueId?: string | null;
  issueTitle?: string | null;
  workDate: string;
  durationMinutes: number;
  durationFormatted: string;
  description: string;
  createdAt: string;
  updatedAt?: string | null;
  canEdit: boolean;
  canDelete: boolean;
}

export interface TimeEntryListItemDto {
  id: string;
  projectId: string;
  projectName: string;
  issueId?: string | null;
  issueTitle?: string | null;
  workDate: string;
  durationMinutes: number;
  durationFormatted: string;
  description: string;
  createdAt: string;
  canViewProject: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

export interface MyTimeEntriesResultDto {
  entries: PaginationResult<TimeEntryListItemDto>;
  totalDurationMinutes: number;
  totalDurationFormatted: string;
}

export interface IssueTimeEntryListItemDto {
  id: string;
  authorUserId: string;
  authorName: string;
  workDate: string;
  durationMinutes: number;
  durationFormatted: string;
  description: string;
  createdAt: string;
  canEdit: boolean;
  canDelete: boolean;
}

export interface IssueTimeEntriesResultDto {
  totalDurationMinutes: number;
  totalDurationFormatted: string;
  contributorNames: string[];
  entries: PaginationResult<IssueTimeEntryListItemDto>;
}

export interface RunningTimerDto {
  projectId?: string | null;
  projectName?: string | null;
  issueId?: string | null;
  issueTitle?: string | null;
  startedAtUtc: string;
  elapsedMinutes: number;
  elapsedFormatted: string;
}

export interface RunningTimerStateDto {
  timer?: RunningTimerDto | null;
}

export interface TimeSettingsDto {
  backdatingLimitDays: number;
  canEdit: boolean;
}

export interface TimeReportRowDto {
  label: string;
  userId?: string | null;
  projectId?: string | null;
  issueId?: string | null;
  secondaryLabel?: string | null;
  totalDurationMinutes: number;
  totalDurationFormatted: string;
}

export interface TimeReportMatrixCellDto {
  totalDurationMinutes: number;
  totalDurationFormatted: string;
}

export interface TimeReportMatrixRowDto {
  userLabel: string;
  userId: string;
  cells: TimeReportMatrixCellDto[];
  rowTotal: TimeReportMatrixCellDto;
}

export interface TimeReportMatrixDto {
  columnLabels: string[];
  columnProjectIds: string[];
  rows: TimeReportMatrixRowDto[];
  columnTotals: TimeReportMatrixCellDto[];
  grandTotal: TimeReportMatrixCellDto;
}

export interface TimeReportResultDto {
  groupingMode: TimeReportGroupingMode;
  totalDurationMinutes: number;
  totalDurationFormatted: string;
  rows: TimeReportRowDto[];
  matrix?: TimeReportMatrixDto | null;
}

export interface TimeEntryAuditLogEntryDto {
  id: string;
  timeEntryId: string;
  operation: TimeEntryAuditOperation;
  actingUserId: string;
  actingUserName?: string | null;
  entryAuthorUserId: string;
  entryAuthorName?: string | null;
  projectId: string;
  projectName: string;
  issueId?: string | null;
  issueTitle?: string | null;
  workDate: string;
  durationMinutes: number;
  durationFormatted: string;
  description: string;
  previousWorkDate?: string | null;
  previousDurationMinutes?: number | null;
  previousDurationFormatted?: string | null;
  previousDescription?: string | null;
  previousProjectId?: string | null;
  previousProjectName?: string | null;
  previousIssueId?: string | null;
  previousIssueTitle?: string | null;
  occurredAt: string;
}

export interface CreateTimeEntryRequest {
  projectId: string;
  issueId?: string | null;
  workDate: string;
  durationMinutes: number;
  description?: string | null;
}

export interface UpdateTimeEntryRequest {
  projectId: string;
  issueId?: string | null;
  workDate: string;
  durationMinutes: number;
  description?: string | null;
}

export interface StartRunningTimerRequest {
  projectId?: string | null;
  issueId?: string | null;
  replaceExisting?: boolean;
}

export interface UpdateTimeSettingsRequest {
  backdatingLimitDays: number;
}

export interface MyTimeEntriesSearchParameters extends PaginationParameters {
  dateFrom?: string;
  dateTo?: string;
  projectId?: string | null;
}

export interface TimeReportsSearchParameters {
  dateFrom: string;
  dateTo: string;
  projectIds?: string[];
  userIds?: string[];
  groupingMode: TimeReportGroupingMode;
}

export interface ReportPersonEntriesSearchParameters extends PaginationParameters {
  userId: string;
  dateFrom: string;
  dateTo: string;
  projectIds?: string[];
}

export interface ReportPersonEntryDto {
  workDate: string;
  projectName: string;
  issueId?: string | null;
  issueTitle?: string | null;
  durationMinutes: number;
  durationFormatted: string;
  description: string;
}

export interface TimeAuditLogSearchParameters extends PaginationParameters {
  dateFrom?: string;
  dateTo?: string;
  actingUserId?: string | null;
  entryAuthorUserId?: string | null;
  projectId?: string | null;
  operations?: TimeEntryAuditOperation[];
}

export interface LoggableProjectOptionDto {
  id: string;
  name: string;
}

export interface LogTimeIssueOptionDto {
  id: string;
  title: string;
}

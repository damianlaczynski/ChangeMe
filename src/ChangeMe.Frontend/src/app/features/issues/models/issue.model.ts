import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface IssueDto {
  id: string;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  createdBy: string;
  createdByName?: string | null;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  lastActivityAt: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
}

export interface IssueAssignableUserDto {
  id: string;
  fullName: string;
}

export interface IssueDetailsDto {
  id: string;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  createdBy: string;
  createdByName?: string | null;
  assignedToUserId?: string | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  lastActivityAt: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
  acceptanceCriteria: AcceptanceCriterionDto[];
  comments: IssueCommentDto[];
  historyEntries: IssueHistoryEntryDto[];
}

export interface AcceptanceCriterionDto {
  id: string;
  content: string;
  createdAt: string;
  createdBy: string;
}

export interface IssueCommentDto {
  id: string;
  content: string;
  authorUserId: string;
  authorName?: string | null;
  createdAt: string;
}

export interface AddIssueCommentRequest {
  content: string;
}

export interface IssueHistoryEntryDto {
  id: string;
  eventType: IssueHistoryEventType;
  actorUserId: string;
  actorName?: string | null;
  summary: string;
  previousValue?: string | null;
  currentValue?: string | null;
  createdAt: string;
}

export interface CreateIssueRequest {
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  watchAfterCreate: boolean;
  acceptanceCriteria: CreateIssueAcceptanceCriterionPayload[];
}

export interface CreateIssueAcceptanceCriterionPayload {
  content: string;
}

export interface UpdateIssueRequest {
  id: string;
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  acceptanceCriteria: UpdateIssueAcceptanceCriterionPayload[];
}

export interface UpdateIssueAcceptanceCriterionPayload {
  id?: string;
  content: string;
}

export interface IssueSearchParameters extends PaginationParameters {
  searchText?: string;
  statuses?: IssueStatus[];
  priorities?: IssuePriority[];
  assignedToUserId?: string | null;
  watchedByMe?: boolean;
  createdByMe?: boolean;
}

export interface IssueWatchStateDto {
  issueId: string;
  isWatchedByCurrentUser: boolean;
  watchersCount: number;
}

export enum IssueStatus {
  NEW = 'NEW',
  IN_PROGRESS = 'IN_PROGRESS',
  RESOLVED = 'RESOLVED',
  CLOSED = 'CLOSED'
}

export enum IssuePriority {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

export enum IssueHistoryEventType {
  ISSUE_CREATED = 'ISSUE_CREATED',
  STATUS_CHANGED = 'STATUS_CHANGED',
  PRIORITY_CHANGED = 'PRIORITY_CHANGED',
  ASSIGNEE_CHANGED = 'ASSIGNEE_CHANGED',
  TITLE_CHANGED = 'TITLE_CHANGED',
  DESCRIPTION_CHANGED = 'DESCRIPTION_CHANGED',
  ACCEPTANCE_CRITERION_ADDED = 'ACCEPTANCE_CRITERION_ADDED',
  ACCEPTANCE_CRITERION_UPDATED = 'ACCEPTANCE_CRITERION_UPDATED',
  ACCEPTANCE_CRITERION_REMOVED = 'ACCEPTANCE_CRITERION_REMOVED'
}

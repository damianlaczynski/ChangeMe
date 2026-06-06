import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export enum ProjectRole {
  OWNER = 'OWNER',
  MEMBER = 'MEMBER',
  VIEWER = 'VIEWER'
}

export enum ProjectMembershipHistoryEventType {
  MEMBER_ADDED = 'MEMBER_ADDED',
  MEMBER_REMOVED = 'MEMBER_REMOVED',
  MEMBER_ROLE_CHANGED = 'MEMBER_ROLE_CHANGED'
}

export enum ProjectOperationHistoryEventType {
  PROJECT_CREATED = 'PROJECT_CREATED',
  NAME_CHANGED = 'NAME_CHANGED',
  DESCRIPTION_CHANGED = 'DESCRIPTION_CHANGED'
}

export interface ProjectListItemDto {
  id: string;
  name: string;
  description: string;
  issueCount: number;
  currentUserRole: ProjectRole;
  isSystem: boolean;
  canManage: boolean;
}

export interface ProjectDetailsDto {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  currentUserRole: ProjectRole;
  issueCount: number;
  canManage: boolean;
  canViewMembers: boolean;
  canManageMembers: boolean;
  canViewIssues: boolean;
  canManageIssues: boolean;
  canViewLoggedTime: boolean;
  loggedTimeMinutes: number;
  loggedTimeFormatted: string;
}

export interface ProjectOptionDto {
  id: string;
  name: string;
}

export interface ProjectMemberDto {
  userId: string;
  firstName: string | null;
  lastName: string | null;
  email: string;
  role: ProjectRole;
  deactivated: boolean;
  canViewUserDetails: boolean;
}

export interface ProjectMembershipHistoryEntryDto {
  id: string;
  eventType: ProjectMembershipHistoryEventType;
  actorUserId: string;
  actorName: string | null;
  affectedUserId: string;
  affectedUserName: string | null;
  summary: string;
  previousValue: string | null;
  currentValue: string | null;
  createdAt: string;
}

export interface ProjectOperationHistoryEntryDto {
  id: string;
  eventType: ProjectOperationHistoryEventType;
  actorUserId: string;
  actorName: string | null;
  summary: string;
  previousValue: string | null;
  currentValue: string | null;
  createdAt: string;
}

export interface ProjectSearchParameters extends PaginationParameters {
  searchText?: string;
}

export interface ProjectMembersSearchParameters extends PaginationParameters {
  searchText?: string;
}

export interface ProjectHistorySearchParameters extends PaginationParameters {
  sortField?: string;
  ascending?: boolean;
}

export interface CreateProjectRequest {
  name: string;
  description?: string | null;
}

export interface UpdateProjectRequest {
  id: string;
  name: string;
  description?: string | null;
}

export interface AddProjectMemberRequest {
  userId: string;
  role: ProjectRole;
}

export interface ChangeProjectMemberRoleRequest {
  role: ProjectRole;
}

import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface ProjectListItemDto {
  id: string;
  name: string;
  key: string;
  description?: string | null;
  status: ProjectStatus;
  visibility: ProjectVisibility;
  color: string;
  issueCount: number;
  memberCount: number;
  currentUserRole?: ProjectMemberRole | null;
}

export interface ProjectSelectionItemDto {
  id: string;
  name: string;
  key: string;
  color: string;
  status: ProjectStatus;
}

export interface ProjectMemberDto {
  userId: string;
  displayLabel: string;
  role: ProjectMemberRole;
  joinedAt: string;
}

export interface ProjectDetailsDto {
  id: string;
  name: string;
  key: string;
  description?: string | null;
  status: ProjectStatus;
  visibility: ProjectVisibility;
  color: string;
  issueCount: number;
  memberCount: number;
  createdAt: string;
  updatedAt?: string | null;
  members: ProjectMemberDto[];
  currentUserRole?: ProjectMemberRole | null;
}

export interface ProjectOverviewDto {
  id: string;
  name: string;
  key: string;
  description?: string | null;
  status: ProjectStatus;
  visibility: ProjectVisibility;
  color: string;
  totalIssues: number;
  newIssues: number;
  inProgressIssues: number;
  resolvedIssues: number;
  closedIssues: number;
  memberCount: number;
}

export interface CreateProjectRequest {
  name: string;
  key: string;
  description: string | null;
  visibility: ProjectVisibility;
  color: string | null;
}

export interface UpdateProjectRequest {
  id: string;
  name: string;
  key: string;
  description: string | null;
  visibility: ProjectVisibility;
  status: ProjectStatus;
  color: string | null;
}

export interface AddProjectMemberRequest {
  userId: string;
  role: ProjectMemberRole;
}

export interface UpdateProjectMemberRoleRequest {
  role: ProjectMemberRole;
}

export interface ProjectSearchParameters extends PaginationParameters {
  searchText?: string;
  statuses?: ProjectStatus[];
}

export enum ProjectStatus {
  ACTIVE = 'ACTIVE',
  ON_HOLD = 'ON_HOLD',
  ARCHIVED = 'ARCHIVED'
}

export enum ProjectVisibility {
  INTERNAL = 'INTERNAL',
  PRIVATE = 'PRIVATE'
}

export enum ProjectMemberRole {
  OWNER = 'OWNER',
  MEMBER = 'MEMBER',
  VIEWER = 'VIEWER'
}

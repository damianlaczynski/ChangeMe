import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export type UserStatus = 'Active' | 'Inactive';

export interface UserListItemDto {
  id: string;
  fullName: string;
  email: string;
  status: UserStatus;
  roleNames: string[];
  lastSignInAt: string | null;
  createdAt: string;
}

export interface UserRoleSummaryDto {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface EffectivePermissionDto {
  code: string;
  label: string;
  description: string;
  group: string;
  fromRoleNames: string[];
}

export interface UserDetailsDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  status: UserStatus;
  memberSince: string;
  lastSignInAt: string | null;
  roles: UserRoleSummaryDto[];
  effectivePermissions: EffectivePermissionDto[];
}

export interface RoleAssignmentOptionDto {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface AdminUserSessionDto {
  id: string;
  deviceBrowserLabel: string;
  ipAddress: string | null;
  isPersistent: boolean;
  signedInAt: string;
  lastActivityAt: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  roleIds: string[];
  status: UserStatus;
}

export interface UpdateUserRequest {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roleIds?: string[];
  status?: UserStatus;
}

export interface UserSearchParameters extends PaginationParameters {
  searchText?: string;
  statuses?: UserStatus[];
}

export interface PreviewEffectivePermissionsRequest {
  roleIds: string[];
}

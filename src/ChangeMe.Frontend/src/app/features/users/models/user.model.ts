import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface UserListItemDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  deactivated: boolean;
  hasPasswordSet: boolean;
  emailVerified: boolean;
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
  email: string;
  deactivated: boolean;
  deactivatedAt: string | null;
  hasPasswordSet: boolean;
  emailVerified: boolean;
  emailVerifiedAt: string | null;
  passwordLastChangedAt: string | null;
  passwordExpiresAtUtc: string | null;
  invitationSentAt: string | null;
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
  roleIds: string[];
}

export interface UpdateUserRequest {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roleIds?: string[];
  deactivated?: boolean;
}

export interface UserSearchParameters extends PaginationParameters {
  searchText?: string;
  deactivated?: boolean[];
  emailVerified?: boolean[];
}

export interface PreviewEffectivePermissionsRequest {
  roleIds: string[];
}

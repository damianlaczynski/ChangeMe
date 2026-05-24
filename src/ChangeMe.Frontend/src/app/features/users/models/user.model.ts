import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface UserListItemDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  deactivated: boolean;
  hasPasswordSet: boolean;
  emailVerified: boolean;
  invitationPending: boolean;
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

export interface UserPasskeyDto {
  id: string;
  name: string;
  createdAtUtc: string;
  lastUsedAtUtc: string | null;
  authenticatorType: string;
  backupEligible: boolean;
  backupState: boolean;
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
  twoFactorEnabled: boolean;
  twoFactorEnabledAt: string | null;
  pendingInvitation: UserInvitationInfoDto | null;
  memberSince: string;
  lastSignInAt: string | null;
  roles: UserRoleSummaryDto[];
  effectivePermissions: EffectivePermissionDto[];
  externalLogins: UserExternalLoginDto[];
  passkeys: UserPasskeyDto[];
}

export interface UserInvitationInfoDto {
  lastSentAtUtc: string;
  sentCount: number;
  expiresAtUtc: string;
}

export interface UserExternalLoginDto {
  providerKey: string;
  displayName: string;
  linkedAtUtc: string;
}

export interface RoleAssignmentOptionDto {
  id: string;
  name: string;
  isSystem: boolean;
}

export interface AdminUserSessionDto {
  id: string;
  deviceBrowserLabel: string;
  signInMethod: string;
  signInMethodLabel: string;
  ipAddress: string | null;
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

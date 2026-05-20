import { UserStatus } from '@features/users/models/user.model';

export interface RoleListItemDto {
  id: string;
  name: string;
  description: string | null;
  permissionCount: number;
  userCount: number;
  isSystem: boolean;
}

export interface PermissionCatalogItemDto {
  code: string;
  label: string;
  description: string;
  group: string;
}

export interface RolePermissionItemDto {
  code: string;
  label: string;
  description: string;
  group: string;
}

export interface RoleDetailsDto {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissionCount: number;
  userCount: number;
  permissions: RolePermissionItemDto[];
}

export interface RoleFormDto {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissionCodes: string[];
}

export interface RoleAssignedUserDto {
  id: string;
  fullName: string;
  email: string;
  status: UserStatus;
}

export interface UserAssignmentOptionDto {
  id: string;
  fullName: string;
  email: string;
  status: UserStatus;
}

export interface ManageRoleUsersFormDto {
  roleId: string;
  roleName: string;
  isSystem: boolean;
  assignedUserIds: string[];
  availableUsers: UserAssignmentOptionDto[];
}

export interface RoleSearchParameters {
  searchText?: string;
  sortField?: string;
  ascending?: boolean;
}

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
  permissionCodes: string[];
}

export interface UpdateRoleRequest {
  id: string;
  name: string;
  description?: string | null;
  permissionCodes: string[];
}

export interface UpdateRoleUsersRequest {
  roleId: string;
  userIds: string[];
}

import { UserStatus } from '@features/users/models/user.model';
import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

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

export interface RoleAssignedUserDto {
  id: string;
  fullName: string;
  email: string;
  status: UserStatus;
}

export interface RoleSearchParameters extends PaginationParameters {
  searchText?: string;
}

export interface RoleAssignedUsersSearchParameters extends PaginationParameters {
  searchText?: string;
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

import { EffectivePermissionDto } from '@features/users/models/user.model';
import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface AuthResponse {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  sessionId: string;
  token: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  permissions: string[];
  isPersistent: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface RefreshSessionRequest {
  refreshToken: string;
}

export interface UserSessionDto {
  id: string;
  deviceBrowserLabel: string;
  ipAddress: string | null;
  isPersistent: boolean;
  signedInAt: string;
  lastActivityAt: string;
  isCurrent: boolean;
}

export interface MyAccountDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  status: string;
  memberSince: string;
  effectivePermissions: EffectivePermissionDto[];
}

export interface UpdateMyAccountRequest {
  firstName: string;
  lastName: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export type UserSessionSearchParameters = PaginationParameters;

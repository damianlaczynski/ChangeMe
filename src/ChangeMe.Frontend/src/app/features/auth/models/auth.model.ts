import {
  EffectivePermissionDto,
  UserRoleSummaryDto
} from '@features/users/models/user.model';

export interface LoginResponse {
  authSession: AuthResponse;
}

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
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshSessionRequest {
  refreshToken: string;
}

export interface MyAccountDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  memberSince: string;
  roles: UserRoleSummaryDto[];
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

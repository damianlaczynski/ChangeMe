import {
  EffectivePermissionDto,
  UserRoleSummaryDto
} from '@features/users/models/user.model';
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
  passwordChangeRequired: boolean;
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

export interface RegisterResponse {
  requiresEmailVerification: boolean;
  authSession: AuthResponse | null;
}

export interface EmailVerificationAck {
  message: string;
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

export interface RequiredChangePasswordRequest {
  newPassword: string;
}

export interface PasswordPolicySettings {
  minimumLength: number;
  maximumLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireDigit: boolean;
  requireSpecialCharacter: boolean;
}

export interface AuthSettings {
  passwordPolicy: PasswordPolicySettings;
  publicRegistrationEnabled: boolean;
  emailVerificationEnabled: boolean;
  passwordExpirationEnabled: boolean;
  maximumPasswordAgeDays: number;
}

export type UserSessionSearchParameters = PaginationParameters;

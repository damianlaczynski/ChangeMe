import {
  EffectivePermissionDto,
  UserRoleSummaryDto
} from '@features/users/models/user.model';
import { PaginationParameters } from '@shared/data/models/pagination-parameters.model';

export interface LoginResponse {
  authSession: AuthResponse | null;
  twoFactorChallenge: PendingSignInChallenge | null;
}

export interface ExternalSignInResponse {
  authSession: AuthResponse | null;
  twoFactorChallenge: PendingSignInChallenge | null;
  linkAccountRequired: ExternalAccountLinkRequired | null;
  accountLinkCompleted: boolean;
  externalStepUpCompleted: boolean;
}

export interface MyAccountExternalLogin {
  providerKey: string;
  displayName: string;
  linkedAtUtc: string;
}

export interface UnlinkExternalAccountRequest {
  currentPassword?: string | null;
  verificationCode?: string | null;
}

export interface SetPasswordRequest {
  newPassword: string;
  currentPassword?: string | null;
  verificationCode?: string | null;
}

export interface ExternalAccountLinkRequired {
  state: string;
  email: string;
  providerKey: string;
  providerDisplayName: string;
}

export interface BeginExternalSignInResponse {
  authorizationUrl: string;
}

export interface CompleteExternalSignInRequest {
  code: string;
  state: string;
}

export interface LinkExternalAccountRequest {
  state: string;
  password: string;
}

export interface PendingSignInChallenge {
  challengeId: string;
}

export interface VerifyTwoFactorRequest {
  challengeId: string;
  verificationCode: string;
}

export interface BeginTwoFactorSetupRequest {
  currentPassword?: string | null;
}

export interface BeginTwoFactorSetupResponse {
  sharedSecret: string;
  provisioningUri: string;
  issuerName: string;
}

export interface ConfirmTwoFactorSetupRequest {
  verificationCode: string;
}

export interface TwoFactorSetupCompletedResponse {
  recoveryCodes: string[];
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
  passwordChangeRequired: boolean;
  passwordChangeStrict: boolean;
  passwordExpiresAtUtc: string | null;
  twoFactorSetupRequired: boolean;
  twoFactorSetupStrict: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
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
  hasPasswordSet: boolean;
  twoFactorEnabled: boolean;
  twoFactorEnabledAt: string | null;
  externalStepUpFresh: boolean;
  roles: UserRoleSummaryDto[];
  effectivePermissions: EffectivePermissionDto[];
  externalLogins: MyAccountExternalLogin[];
  linkableProviders: ExternalProviderSettings[];
}

export interface DisableTwoFactorRequest {
  currentPassword?: string | null;
  verificationCode?: string | null;
}

export interface TwoFactorStepUpRequest {
  currentPassword?: string | null;
  verificationCode?: string | null;
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

export interface TwoFactorSettings {
  verificationCodeLength: number;
  recoveryCodeCount: number;
  totpTimeStepSeconds: number;
  stepUpExternalSignInValidityMinutes: number;
}

export interface ExternalProviderSettings {
  providerKey: string;
  displayName: string;
}

export interface AuthSettings {
  passwordPolicy: PasswordPolicySettings;
  publicRegistrationEnabled: boolean;
  emailVerificationEnabled: boolean;
  passwordExpirationEnabled: boolean;
  maximumPasswordAgeDays: number;
  twoFactorAuthenticationEnabled: boolean;
  twoFactorAuthenticationRequired: boolean;
  trustIdentityProviderMfa: boolean;
  externalProvidersEnabled: boolean;
  twoFactor: TwoFactorSettings;
  externalProviders: ExternalProviderSettings[];
}

export type UserSessionSearchParameters = PaginationParameters;

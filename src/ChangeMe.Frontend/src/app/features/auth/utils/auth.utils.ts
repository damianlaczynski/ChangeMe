export const AuthConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128,
  RENEWAL_LEAD_TIME_MS: 60_000,
  MIN_RENEWAL_SCHEDULE_MS: 5_000,
  TWO_FACTOR_VERIFICATION_CODE_LENGTH: 6,
  TWO_FACTOR_RECOVERY_CODE_COUNT: 10,
  TWO_FACTOR_TOTP_TIME_STEP_SECONDS: 30
};

export const AuthMessages = {
  invalidCredentials: 'Invalid email or password.',
  deactivatedAccount: 'This account has been deactivated. Contact an administrator.',
  duplicateEmail: 'An account with this email already exists.',
  passwordChangedLogin: 'Password changed. Sign in with your new password.',
  accountActivatedLogin: 'Account activated. Sign in with your new password.',
  invalidInvitationLink:
    'This invitation link is invalid or has expired. Contact your administrator.',
  forgotPasswordSuccess:
    'If an account exists for this email, a reset link has been sent.',
  invalidPasswordResetLink:
    'This reset link is invalid or has expired. Request a new link from the sign-in page.',
  passwordResetLogin: 'Password reset. Sign in with your new password.',
  signOutEverywhereTitle: 'Sign out from all devices?',
  signOutEverywhereMessage:
    'Sign out from all devices? You will be signed out on every browser and device.',
  revokeSessionTitle: 'Revoke this session?',
  revokeSessionMessage: 'Revoke this session? That device will be signed out.',
  changePasswordTitle: 'Change password and sign out everywhere?',
  changePasswordMessage:
    'Change password and sign out everywhere? You will be signed out on every device and must sign in again with your new password.',
  changePasswordNotice:
    'Changing your password will sign you out on all devices, including this one. You will need to sign in again with your new password.',
  requiredPasswordChangeTitle: 'Required password change',
  requiredPasswordChangeSubtitle:
    'Your password has expired. Set a new password to continue using the application.',
  passwordExpiringSoon: 'Password expiring soon',
  passwordExpiredSummary: 'Password expired',
  passwordExpiredDetail:
    'Your password has expired. Set a new password to save your work to the server.',
  passwordUpdated: 'Password updated.',
  emailNotVerified: 'Verify your email before signing in.',
  registrationDisabled: 'Registration is disabled. Contact an administrator.',
  accountCreatedVerifyEmail:
    'Account created. Check your email to verify your address before signing in.',
  emailVerificationResendSuccess:
    'If an unverified account exists for this email, a verification link has been sent.',
  invalidEmailVerificationLink: 'This verification link is invalid or has expired.',
  emailVerifiedLogin: 'Email verified. You can sign in now.',
  twoFactorVerificationTitle: 'Two-factor verification',
  twoFactorVerificationSubtitle:
    'Enter the verification code from your authenticator app or use a recovery code.',
  twoFactorUseRecoveryCodeHint:
    'You can enter a recovery code instead of a verification code.',
  invalidVerificationCode: 'Invalid verification code.',
  signInTimedOut: 'Sign-in timed out. Try again.',
  tooManyAttempts: 'Too many attempts. Sign in again.',
  twoFactorSetupRequiredTitle: 'Set up two-factor authentication',
  twoFactorSetupRequiredSubtitle:
    'Two-factor authentication is required for your account before you can continue.',
  twoFactorEnabled: 'Two-factor authentication enabled.',
  twoFactorDisabled: 'Two-factor authentication disabled.',
  twoFactorSetupRequiredSummary: 'Two-factor authentication required',
  twoFactorSetupRequiredDetail:
    'Set up two-factor authentication to continue saving your work to the server.',
  twoFactorSetupRequiredAction: 'Set up now',
  twoFactorRequiredWarning: 'Two-factor authentication is required for your account.',
  twoFactorScanQrHint:
    'Scan this QR code with your authenticator app, then enter the verification code below.',
  twoFactorQrUnavailable:
    'Unable to generate a QR code. Use the manual entry key instead.',
  twoFactorShowManualKey: 'Enter key manually instead',
  twoFactorHideManualKey: 'Hide manual entry key',
  twoFactorManualKeyLabel: 'Manual entry key',
  twoFactorIssuerLabel: 'Issuer',
  recoveryCodesRegenerated: 'Recovery codes regenerated.',
  profileUpdated: 'Profile updated.',
  noActiveSessions: 'No active sessions.',
  permissionDenied: 'You do not have permission to perform this action.',
  externalSignInFailed: 'External sign-in failed. Try again or use email and password.',
  signInNotAllowed: 'Sign-in with this account is not allowed.',
  noAccountExists: 'No account exists for this email. Contact an administrator.',
  externalAccountAlreadyLinked:
    'This external account is already linked to another user.',
  externalSignInUnavailable:
    'External sign-in is unavailable. Contact an administrator or set a password when sign-in is available.',
  linkExternalAccountTitle: 'Link external account',
  linkExternalAccountSubtitle:
    'An account with this email already exists. Enter your password to link sign-in.',
  externalSignInCallbackLoading: 'Completing sign-in...',
  externalSignInMethodsTitle: 'External sign-in methods',
  externalAccountLinked: 'External sign-in method linked.',
  externalAccountUnlinked: 'External sign-in method removed.',
  unlinkExternalProviderTitle: 'Remove external sign-in?',
  unlinkExternalProviderMessage: (displayName: string) =>
    `Remove ${displayName} sign-in from your account?`,
  setPasswordTitle: 'Set password',
  setPasswordSubtitle:
    'Create a password so you can also sign in with email and use forgot password.',
  passwordSet: 'Password set.',
  externalStepUpRequired:
    'Complete sign-in with a linked external provider to continue.',
  externalProviderEmailMismatch:
    'The external account email does not match your account email.',
  cannotRemoveOnlySignInMethod:
    'Set a password before removing your only sign-in method.',
  verifyWithProvider: 'Verify with provider'
};

export { PermissionCodes } from '@shared/authorization/permission-codes';

export function formatIpAddress(ipAddress: string | null | undefined): string {
  return ipAddress?.trim() ? ipAddress : 'Unknown';
}

export function passwordExpiryWarningDetail(daysRemaining: number): string {
  const dayLabel = daysRemaining === 1 ? '1 day' : `${daysRemaining} days`;
  return `Your password expires in ${dayLabel}. Change it now to avoid interruption.`;
}

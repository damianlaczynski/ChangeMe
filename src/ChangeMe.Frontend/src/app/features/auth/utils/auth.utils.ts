export const AuthConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128,
  RENEWAL_LEAD_TIME_MS: 60_000,
  MIN_RENEWAL_SCHEDULE_MS: 5_000
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
  profileUpdated: 'Profile updated.',
  noActiveSessions: 'No active sessions.',
  permissionDenied: 'You do not have permission to perform this action.'
};

export { PermissionCodes } from '@shared/authorization/permission-codes';

export function formatSessionType(isPersistent: boolean): string {
  return isPersistent ? 'Persistent' : 'Browser';
}

export function formatIpAddress(ipAddress: string | null | undefined): string {
  return ipAddress?.trim() ? ipAddress : 'Unknown';
}

export function passwordExpiryWarningDetail(daysRemaining: number): string {
  const dayLabel = daysRemaining === 1 ? '1 day' : `${daysRemaining} days`;
  return `Your password expires in ${dayLabel}. Change it now to avoid interruption.`;
}

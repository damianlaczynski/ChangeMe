import { highlightDialogValue } from '@shared/ui/utils/dialog-message.utils';

export const UserConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128
};

export const UserMessages = {
  duplicateEmail: 'A user with this email already exists.',
  userCreated: 'User created. An invitation email has been sent.',
  invitationResent: 'Invitation resent.',
  passwordResetSent: 'Password reset email sent.',
  emailMarkedAsVerified: 'Email marked as verified.',
  confirmEmailTitle: 'Confirm email',
  confirmEmailMessage: (userReference: string) =>
    `Mark email as verified for ${highlightDialogValue(userReference)}?`,
  sendPasswordResetTitle: 'Send password reset?',
  sendPasswordResetMessage: (email: string) =>
    `Send a password reset link to ${highlightDialogValue(email)}?`,
  resendInvitationTitle: 'Resend invitation?',
  resendInvitationMessage: (email: string) =>
    `Resend invitation to ${highlightDialogValue(email)}? A new invitation link will be sent. Previous unused links will stop working.`,
  userSaved: 'User saved.',
  userDeactivated: 'User deactivated.',
  userActivated: 'User activated.',
  profileUpdated: 'Profile updated.',
  noRolesAssigned: 'No roles assigned.',
  noPermissions: 'No permissions.',
  selectRoleToPreview: 'Select at least one role to preview permissions.',
  neverSignedIn: 'Never',
  noActiveSessions: 'No active sessions.',
  revokeSessionTitle: 'Revoke this session?',
  revokeSessionMessage: 'Revoke this session? That device will be signed out.',
  revokeAllSessionsTitle: 'Revoke all active sessions for this user?',
  revokeAllSessionsMessage:
    'Revoke all active sessions for this user? They will be signed out on every device.'
};

export const accountFilters: { label: string; value: boolean }[] = [
  { label: 'Active', value: false },
  { label: 'Deactivated', value: true }
];

export const emailVerifiedFilters: { label: string; value: boolean }[] = [
  { label: 'Verified', value: true },
  { label: 'Unverified', value: false }
];

export function getAccountBadgeLabel(deactivated: boolean): string {
  return deactivated ? 'Deactivated' : 'Active';
}

export function getAccountBadgeSeverity(deactivated: boolean): 'success' | 'danger' {
  return deactivated ? 'danger' : 'success';
}

export function getEmailVerifiedBadgeLabel(verified: boolean): string {
  return verified ? 'Verified' : 'Unverified';
}

export function getEmailVerifiedBadgeSeverity(verified: boolean): 'success' | 'warn' {
  return verified ? 'success' : 'warn';
}

export function getAccountStateLabel(
  user: {
    deactivated: boolean;
    hasPasswordSet: boolean;
    emailVerified: boolean;
  },
  emailVerificationEnabled = false
): string | null {
  if (user.deactivated) {
    return null;
  }

  if (!user.hasPasswordSet) {
    return 'Awaiting invitation';
  }

  if (emailVerificationEnabled && !user.emailVerified) {
    return 'Awaiting email verification';
  }

  return 'Complete';
}

export function formatFromRoles(roleNames: string[]): string {
  if (roleNames.length === 0) {
    return '';
  }

  if (roleNames.length === 1) {
    return `From role: ${roleNames[0]}`;
  }

  return `From roles: ${roleNames.join(', ')}`;
}

export function getDeactivateConfirmMessage(userReference: string): string {
  return `Deactivate ${highlightDialogValue(userReference)}? The user will be signed out and cannot sign in until reactivated.`;
}

export function getActivateConfirmMessage(userReference: string): string {
  return `Activate ${highlightDialogValue(userReference)}? The user will be able to sign in again.`;
}

export function groupEffectivePermissions<T extends { group: string }>(
  permissions: T[]
): { group: string; items: T[] }[] {
  const groups = new Map<string, T[]>();

  for (const permission of permissions) {
    const existing = groups.get(permission.group) ?? [];
    existing.push(permission);
    groups.set(permission.group, existing);
  }

  return [...groups.entries()].map(([group, items]) => ({ group, items }));
}

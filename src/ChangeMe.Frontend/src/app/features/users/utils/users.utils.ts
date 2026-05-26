import { highlightDialogValue } from '@shared/ui/utils/dialog-message.utils';

export const UserConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128
};

export type UserMembershipStatus =
  | 'Invited'
  | 'InvitationCanceled'
  | 'Active'
  | 'Deactivated';

export function isInvitationLinkExpired(
  expiresAtUtc: string,
  utcNowMs = Date.now()
): boolean {
  const expiresAtMs = new Date(expiresAtUtc).getTime();
  if (Number.isNaN(expiresAtMs)) {
    return true;
  }

  return expiresAtMs <= utcNowMs;
}

export const UserMessages = {
  externalLoginEmailWarning:
    'This user has external sign-in linked. Changing email does not remove external logins; the user signs in by provider identity, not email match.',
  duplicateEmail: 'A user with this email already exists.',
  invitationSent: 'Invitation sent.',
  invitationResent: 'Invitation resent.',
  invitationCanceled: 'Invitation canceled.',
  cancelInvitationTitle: 'Cancel invitation?',
  cancelInvitationMessage: (email: string) =>
    `Cancel invitation for ${highlightDialogValue(email)}? They will not be able to use the current invitation link. You can send a new invitation later.`,
  sendInvitationTitle: 'Send invitation?',
  sendInvitationMessage: (email: string) =>
    `Send invitation to ${highlightDialogValue(email)}?`,
  invitationPanelIntro:
    'This user was invited and has not completed account setup yet. They cannot sign in until they accept the invitation.',
  invitationExpiryNote:
    'Based on the active invitation link. Changing Auth:Invitations:InvitationLinkLifetimeHours does not change an already-issued token.',
  invitationExpiredMessage:
    'This invitation link may no longer work. Resend or cancel the invitation.',
  passwordResetSent: 'Password reset email sent.',
  emailMarkedAsVerified: 'Email marked as verified.',
  confirmEmailTitle: 'Confirm email',
  confirmEmailMessage: (userReference: string) =>
    `Mark email as verified for ${highlightDialogValue(userReference)}?`,
  sendPasswordResetTitle: 'Send password reset?',
  sendPasswordResetMessage: (email: string) =>
    `Send a password reset link to ${highlightDialogValue(email)}?`,
  resetTwoFactorTitle: 'Reset two-factor authentication?',
  resetTwoFactorMessage: (userReference: string) =>
    `Reset two-factor authentication for ${highlightDialogValue(userReference)}? They will be signed out on every device and must set up two-factor again if required.`,
  twoFactorReset: 'Two-factor authentication reset.',
  resetPasskeysTitle: 'Reset passkeys?',
  resetPasskeysMessage: (userReference: string) =>
    `Remove all passkeys for ${highlightDialogValue(userReference)}? They will be signed out on every device and must register a passkey again if required.`,
  passkeysReset: 'Passkeys reset.',
  removePasskeyTitle: 'Remove passkey?',
  removePasskeyMessage: (name: string) =>
    `Remove passkey ${highlightDialogValue(name)} from this account?`,
  passkeyRemoved: 'Passkey removed.',
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

export const statusFilters: { label: string; value: UserMembershipStatus }[] = [
  { label: 'Invited', value: 'Invited' },
  { label: 'Invitation canceled', value: 'InvitationCanceled' },
  { label: 'Active', value: 'Active' },
  { label: 'Deactivated', value: 'Deactivated' }
];

export const emailVerifiedFilters: { label: string; value: boolean }[] = [
  { label: 'Verified', value: true },
  { label: 'Unverified', value: false }
];

export function getUserStatusLabel(status: UserMembershipStatus): string {
  switch (status) {
    case 'Invited':
      return 'Invited';
    case 'InvitationCanceled':
      return 'Invitation canceled';
    case 'Deactivated':
      return 'Deactivated';
    default:
      return 'Active';
  }
}

/** @deprecated Use getUserStatusLabel for full membership status; kept for role-details user rows. */
export function getAccountBadgeLabel(deactivated: boolean): string {
  return deactivated ? 'Deactivated' : 'Active';
}

/** @deprecated Use getUserStatusSeverity for full membership status; kept for role-details user rows. */
export function getAccountBadgeSeverity(deactivated: boolean): 'success' | 'danger' {
  return deactivated ? 'danger' : 'success';
}

export function getUserStatusSeverity(
  status: UserMembershipStatus
): 'success' | 'danger' | 'warn' | 'secondary' {
  switch (status) {
    case 'Deactivated':
      return 'danger';
    case 'Invited':
      return 'warn';
    case 'InvitationCanceled':
      return 'secondary';
    default:
      return 'success';
  }
}

export function getEmailVerifiedBadgeLabel(verified: boolean): string {
  return verified ? 'Verified' : 'Unverified';
}

export function getEmailVerifiedBadgeSeverity(verified: boolean): 'success' | 'warn' {
  return verified ? 'success' : 'warn';
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

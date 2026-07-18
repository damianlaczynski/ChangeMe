import { InvitationStatus } from '@features/invitations/models/invitation.model';

export const InvitationMessages = {
  sent: 'Invitation sent.',
  resent: 'Invitation resent.',
  revoked: 'Invitation revoked.',
  accountCreated: 'Account created. Sign in with your email and password.'
} as const;

export const InvitationConstraints = {
  EMAIL_MAX_LENGTH: 320,
  NAME_MAX_LENGTH: 100
} as const;

export function getRevokeConfirmMessage(email: string): string {
  return `Revoke invitation for "${email}"? The link will stop working.`;
}

export function getInvitationStatusLabel(status: InvitationStatus): string {
  switch (status) {
    case 'PENDING':
      return 'Pending';
    case 'ACCEPTED':
      return 'Accepted';
    case 'EXPIRED':
      return 'Expired';
    case 'REVOKED':
      return 'Revoked';
    default:
      return status;
  }
}

export function getInvitationStatusSeverity(
  status: InvitationStatus
): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
  switch (status) {
    case 'PENDING':
      return 'info';
    case 'ACCEPTED':
      return 'success';
    case 'EXPIRED':
      return 'warn';
    case 'REVOKED':
      return 'danger';
    default:
      return 'secondary';
  }
}

export const invitationStatusFilters = [
  { label: 'Pending', value: 'PENDING' },
  { label: 'Accepted', value: 'ACCEPTED' },
  { label: 'Expired', value: 'EXPIRED' },
  { label: 'Revoked', value: 'REVOKED' }
];

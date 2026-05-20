import { UserStatus } from '../models/user.model';

export const UserConstraints = {
  NAME_MAX_LENGTH: 100,
  EMAIL_MAX_LENGTH: 320,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128
};

export const UserMessages = {
  duplicateEmail: 'A user with this email already exists.',
  userCreated: 'User created.',
  userSaved: 'User saved.',
  userDeactivated: 'User deactivated.',
  userActivated: 'User activated.',
  profileUpdated: 'Profile updated.',
  noRolesAssigned: 'No roles assigned.',
  noPermissions: 'No permissions.',
  selectRoleToPreview: 'Select at least one role to preview effective permissions.',
  neverSignedIn: 'Never',
  noActiveSessions: 'No active sessions.',
  revokeSessionTitle: 'Revoke this session?',
  revokeSessionMessage: 'Revoke this session? That device will be signed out.',
  revokeAllSessionsTitle: 'Revoke all active sessions for this user?',
  revokeAllSessionsMessage:
    'Revoke all active sessions for this user? They will be signed out on every device.'
};

export const userStatuses: { label: string; value: UserStatus }[] = [
  { label: 'Active', value: 'Active' },
  { label: 'Inactive', value: 'Inactive' }
];

export function getUserStatusSeverity(status: UserStatus): 'success' | 'danger' {
  return status === 'Active' ? 'success' : 'danger';
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

export function getDeactivateConfirmMessage(fullName: string): string {
  return `Deactivate "${fullName}"? The user will be signed out and cannot sign in until reactivated.`;
}

export function getActivateConfirmMessage(fullName: string): string {
  return `Activate "${fullName}"? The user will be able to sign in again.`;
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

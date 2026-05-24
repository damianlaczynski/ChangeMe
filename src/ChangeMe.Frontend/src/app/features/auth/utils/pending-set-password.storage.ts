const PENDING_SET_PASSWORD_KEY = 'changeMe.pendingSetPassword';

export interface PendingSetPassword {
  newPassword: string;
}

export function storePendingSetPassword(payload: PendingSetPassword): void {
  sessionStorage.setItem(PENDING_SET_PASSWORD_KEY, JSON.stringify(payload));
}

export function readPendingSetPassword(): PendingSetPassword | null {
  const raw = sessionStorage.getItem(PENDING_SET_PASSWORD_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as PendingSetPassword;
    if (typeof parsed.newPassword === 'string' && parsed.newPassword.length > 0) {
      return parsed;
    }
  } catch {
    return null;
  }

  return null;
}

export function clearPendingSetPassword(): void {
  sessionStorage.removeItem(PENDING_SET_PASSWORD_KEY);
}

export function hasPendingSetPassword(): boolean {
  return readPendingSetPassword() !== null;
}

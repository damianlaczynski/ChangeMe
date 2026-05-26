const PENDING_SET_PASSWORD_KEY = 'changeMe.pendingSetPassword';

export function storePendingSetPassword(): void {
  sessionStorage.setItem(PENDING_SET_PASSWORD_KEY, '1');
}

export function clearPendingSetPassword(): void {
  sessionStorage.removeItem(PENDING_SET_PASSWORD_KEY);
}

export function hasPendingSetPassword(): boolean {
  return sessionStorage.getItem(PENDING_SET_PASSWORD_KEY) === '1';
}

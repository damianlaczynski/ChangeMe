const PENDING_PASSKEY_ENROLLMENT_KEY = 'changeMe.pendingPasskeyEnrollment';

export function storePendingPasskeyEnrollmentOffer(): void {
  sessionStorage.setItem(PENDING_PASSKEY_ENROLLMENT_KEY, '1');
}

export function hasPendingPasskeyEnrollmentOffer(): boolean {
  return sessionStorage.getItem(PENDING_PASSKEY_ENROLLMENT_KEY) === '1';
}

export function clearPendingPasskeyEnrollmentOffer(): void {
  sessionStorage.removeItem(PENDING_PASSKEY_ENROLLMENT_KEY);
}

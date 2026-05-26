const PENDING_UNLINK_KEY = 'changeMe.pendingUnlinkProvider';

export function storePendingUnlinkProvider(providerKey: string): void {
  sessionStorage.setItem(PENDING_UNLINK_KEY, providerKey);
}

export function readPendingUnlinkProvider(): string | null {
  return sessionStorage.getItem(PENDING_UNLINK_KEY);
}

export function clearPendingUnlinkProvider(): void {
  sessionStorage.removeItem(PENDING_UNLINK_KEY);
}

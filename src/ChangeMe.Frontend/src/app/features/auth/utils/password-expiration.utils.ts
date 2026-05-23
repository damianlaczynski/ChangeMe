export const PASSWORD_EXPIRED_API_MESSAGE =
  'Your password has expired. Set a new password to continue.';

export const PASSWORD_EXPIRY_WARNING_THRESHOLDS_DAYS = [14, 7, 1] as const;

const WARNING_STORAGE_PREFIX = 'changeme-pwd-warn';

export function getCalendarDaysUntilExpiry(
  passwordExpiresAtUtc: string,
  now = new Date()
): number {
  const expiresAt = new Date(passwordExpiresAtUtc);
  const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const startOfExpiryDay = new Date(
    expiresAt.getFullYear(),
    expiresAt.getMonth(),
    expiresAt.getDate()
  );

  return Math.round(
    (startOfExpiryDay.getTime() - startOfToday.getTime()) / (24 * 60 * 60 * 1000)
  );
}

export function getActiveWarningThreshold(
  daysUntilExpiry: number
): (typeof PASSWORD_EXPIRY_WARNING_THRESHOLDS_DAYS)[number] | null {
  if (daysUntilExpiry <= 0) {
    return null;
  }

  return (
    PASSWORD_EXPIRY_WARNING_THRESHOLDS_DAYS.find(
      (threshold) => daysUntilExpiry === threshold
    ) ?? null
  );
}

export function getWarningStorageKey(userId: string, thresholdDays: number): string {
  return `${WARNING_STORAGE_PREFIX}-${userId}-${thresholdDays}`;
}

export function hasShownWarning(userId: string, thresholdDays: number): boolean {
  return sessionStorage.getItem(getWarningStorageKey(userId, thresholdDays)) === '1';
}

export function markWarningShown(userId: string, thresholdDays: number): void {
  sessionStorage.setItem(getWarningStorageKey(userId, thresholdDays), '1');
}

export function clearWarningMarkers(userId: string | undefined): void {
  if (!userId) {
    return;
  }

  for (const threshold of PASSWORD_EXPIRY_WARNING_THRESHOLDS_DAYS) {
    sessionStorage.removeItem(getWarningStorageKey(userId, threshold));
  }
}

export function isPasswordExpiredApiError(error: unknown): boolean {
  if (!(error instanceof Error)) {
    return false;
  }

  return error.message.includes('password has expired');
}

export function shouldUseSoftExpiryUx(routerUrl: string): boolean {
  return !routerUrl.startsWith('/required-password-change');
}

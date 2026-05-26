const PENDING_TWO_FACTOR_STEP_UP_KEY = 'changeMe.pendingTwoFactorStepUp';

export type PendingTwoFactorStepUpAction = 'disable' | 'regenerate';

export function storePendingTwoFactorStepUp(
  action: PendingTwoFactorStepUpAction
): void {
  sessionStorage.setItem(PENDING_TWO_FACTOR_STEP_UP_KEY, action);
}

export function readPendingTwoFactorStepUp(): PendingTwoFactorStepUpAction | null {
  const raw = sessionStorage.getItem(PENDING_TWO_FACTOR_STEP_UP_KEY);
  if (raw === 'disable' || raw === 'regenerate') {
    return raw;
  }

  return null;
}

export function clearPendingTwoFactorStepUp(): void {
  sessionStorage.removeItem(PENDING_TWO_FACTOR_STEP_UP_KEY);
}

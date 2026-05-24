const PENDING_TWO_FACTOR_STEP_UP_KEY = 'changeMe.pendingTwoFactorStepUp';

export type PendingTwoFactorStepUpAction = 'disable' | 'regenerate';

export interface PendingTwoFactorStepUp {
  action: PendingTwoFactorStepUpAction;
  verificationCode: string;
}

export function storePendingTwoFactorStepUp(payload: PendingTwoFactorStepUp): void {
  sessionStorage.setItem(PENDING_TWO_FACTOR_STEP_UP_KEY, JSON.stringify(payload));
}

export function readPendingTwoFactorStepUp(): PendingTwoFactorStepUp | null {
  const raw = sessionStorage.getItem(PENDING_TWO_FACTOR_STEP_UP_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as PendingTwoFactorStepUp;
    if (
      (parsed.action === 'disable' || parsed.action === 'regenerate') &&
      typeof parsed.verificationCode === 'string'
    ) {
      return parsed;
    }
  } catch {
    return null;
  }

  return null;
}

export function clearPendingTwoFactorStepUp(): void {
  sessionStorage.removeItem(PENDING_TWO_FACTOR_STEP_UP_KEY);
}

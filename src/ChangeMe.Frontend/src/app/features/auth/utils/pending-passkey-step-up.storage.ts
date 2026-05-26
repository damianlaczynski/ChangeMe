const PENDING_PASSKEY_STEP_UP_KEY = 'changeMe.pendingPasskeyStepUp';

export type PendingPasskeyStepUpAction = 'add' | 'rename' | 'remove';

export interface PendingPasskeyStepUp {
  action: PendingPasskeyStepUpAction;
  verificationCode: string;
  passkeyId?: string;
  passkeyName?: string;
}

export function storePendingPasskeyStepUp(payload: PendingPasskeyStepUp): void {
  sessionStorage.setItem(PENDING_PASSKEY_STEP_UP_KEY, JSON.stringify(payload));
}

export function readPendingPasskeyStepUp(): PendingPasskeyStepUp | null {
  const raw = sessionStorage.getItem(PENDING_PASSKEY_STEP_UP_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as PendingPasskeyStepUp;
    if (
      (parsed.action === 'add' ||
        parsed.action === 'rename' ||
        parsed.action === 'remove') &&
      typeof parsed.verificationCode === 'string'
    ) {
      return parsed;
    }
  } catch {
    return null;
  }

  return null;
}

export function clearPendingPasskeyStepUp(): void {
  sessionStorage.removeItem(PENDING_PASSKEY_STEP_UP_KEY);
}

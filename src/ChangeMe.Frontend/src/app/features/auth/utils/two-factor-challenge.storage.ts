const TWO_FACTOR_CHALLENGE_STORAGE_KEY = 'changeMe.twoFactorChallenge';

export interface StoredTwoFactorChallenge {
  challengeId: string;
}

export function storeTwoFactorChallenge(challenge: StoredTwoFactorChallenge): void {
  sessionStorage.setItem(TWO_FACTOR_CHALLENGE_STORAGE_KEY, JSON.stringify(challenge));
}

export function readTwoFactorChallenge(): StoredTwoFactorChallenge | null {
  const rawValue = sessionStorage.getItem(TWO_FACTOR_CHALLENGE_STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    const parsed = JSON.parse(rawValue) as StoredTwoFactorChallenge;
    if (!parsed.challengeId) {
      return null;
    }

    return parsed;
  } catch {
    clearTwoFactorChallenge();
    return null;
  }
}

export function clearTwoFactorChallenge(): void {
  sessionStorage.removeItem(TWO_FACTOR_CHALLENGE_STORAGE_KEY);
}

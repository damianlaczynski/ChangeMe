import { ExternalAccountLinkRequired } from '@features/auth/models/auth.model';

const STORAGE_KEY = 'changeMe.externalLinkRequired';

export function storeExternalLinkRequired(link: ExternalAccountLinkRequired): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(link));
}

export function readExternalLinkRequired(): ExternalAccountLinkRequired | null {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as ExternalAccountLinkRequired;
  } catch {
    return null;
  }
}

export function clearExternalLinkRequired(): void {
  sessionStorage.removeItem(STORAGE_KEY);
}

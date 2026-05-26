import {
  browserSupportsWebAuthn,
  startAuthentication,
  startRegistration
} from '@simplewebauthn/browser';

export function isPasskeySupported(): boolean {
  return browserSupportsWebAuthn();
}

export function isWebAuthnCeremonyCancelled(error: unknown): boolean {
  if (!error || typeof error !== 'object' || !('name' in error)) {
    return false;
  }

  const name = String((error as { name: unknown }).name);
  return name === 'NotAllowedError' || name === 'AbortError';
}

export function getPasskeyCeremonyErrorMessage(
  error: unknown,
  fallback: string
): string | null {
  if (isWebAuthnCeremonyCancelled(error)) {
    return null;
  }

  return error instanceof Error ? error.message : fallback;
}

export const PASSKEY_SETUP_REQUIRED_API_MESSAGE =
  'Passkey setup is required to continue.';

export function isPasskeySetupRequiredApiError(error: unknown): boolean {
  if (!(error instanceof Error)) {
    return false;
  }

  return error.message.includes('Passkey setup is required');
}

export function shouldUseSoftPasskeyUx(routerUrl: string): boolean {
  return !routerUrl.startsWith('/required-passkey-setup');
}

export function canOfferOptionalPasskeyEnrollment(
  passkeysEnabled: boolean,
  enrollmentPromptEnabled: boolean,
  passkeyCount: number
): boolean {
  return (
    passkeysEnabled &&
    enrollmentPromptEnabled &&
    passkeyCount === 0 &&
    isPasskeySupported()
  );
}

export async function performPasskeyRegistration(options: unknown): Promise<unknown> {
  return startRegistration({
    optionsJSON: options as Parameters<typeof startRegistration>[0]['optionsJSON']
  });
}

export async function performPasskeyAuthentication(options: unknown): Promise<unknown> {
  return startAuthentication({
    optionsJSON: options as Parameters<typeof startAuthentication>[0]['optionsJSON']
  });
}

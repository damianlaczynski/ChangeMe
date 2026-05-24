import {
  browserSupportsWebAuthn,
  startAuthentication,
  startRegistration
} from '@simplewebauthn/browser';

export function isPasskeySupported(): boolean {
  return browserSupportsWebAuthn();
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

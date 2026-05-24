export const TWO_FACTOR_SETUP_REQUIRED_API_MESSAGE =
  'Two-factor authentication setup is required to continue.';

export function isTwoFactorSetupRequiredApiError(error: unknown): boolean {
  if (!(error instanceof Error)) {
    return false;
  }

  return error.message.includes('Two-factor authentication setup is required');
}

export function shouldUseSoftTwoFactorUx(routerUrl: string): boolean {
  return !routerUrl.startsWith('/required-two-factor-setup');
}

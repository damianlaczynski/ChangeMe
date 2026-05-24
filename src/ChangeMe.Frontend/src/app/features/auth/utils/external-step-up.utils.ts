import { MyAccountDto } from '@features/auth/models/auth.model';

export function needsExternalReauth(account: MyAccountDto): boolean {
  return (
    !account.hasPasswordSet &&
    account.externalLogins.length > 0 &&
    !account.externalStepUpFresh
  );
}

export function buildExternalReauthRequiredDetail(validityMinutes: number): string {
  return `Before continuing, verify your identity with your linked provider. This is required when you have not signed in with that provider in the last ${validityMinutes} minutes.`;
}

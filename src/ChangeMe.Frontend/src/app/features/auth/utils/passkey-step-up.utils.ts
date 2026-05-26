import { MyAccountDto } from '@features/auth/models/auth.model';

export function canOfferPasskeyStepUp(
  account: MyAccountDto,
  passkeysEnabled: boolean
): boolean {
  return passkeysEnabled && account.passkeys.length > 0;
}

export function requiresVerificationCodeStepUp(account: MyAccountDto): boolean {
  return account.twoFactorEnabled;
}

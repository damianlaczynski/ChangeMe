export interface StepUpVerificationResult {
  currentPassword: string | null;
  verificationCode: string | null;
  passkeyVerified: boolean;
}

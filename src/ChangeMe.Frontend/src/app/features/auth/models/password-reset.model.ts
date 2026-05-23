export interface PasswordResetAck {
  message: string;
}

export interface PasswordResetPreview {
  isValid: boolean;
}

export interface RequestPasswordResetRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

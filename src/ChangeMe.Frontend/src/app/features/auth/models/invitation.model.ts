export interface InvitationPreview {
  isValid: boolean;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AcceptInvitationRequest {
  token: string;
  firstName: string;
  lastName: string;
  password: string;
}

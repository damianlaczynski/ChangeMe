export type InvitationStatus = 'PENDING' | 'ACCEPTED' | 'EXPIRED' | 'REVOKED';

export interface InvitationListItemDto {
  id: string;
  email: string;
  status: InvitationStatus;
  roleNames: string[];
  invitedByUserId: string;
  invitedByName?: string | null;
  createdAt: string;
  expiresAt: string;
  acceptedAt?: string | null;
}

export interface InvitationAcceptanceDetailsDto {
  email: string;
  firstName?: string | null;
  lastName?: string | null;
}

export interface CreateInvitationRequest {
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  roleIds?: string[] | null;
}

export interface AcceptInvitationRequest {
  firstName: string;
  lastName: string;
  password: string;
}

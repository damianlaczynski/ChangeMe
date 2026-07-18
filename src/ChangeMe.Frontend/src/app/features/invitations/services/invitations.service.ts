import { Injectable, inject } from '@angular/core';
import {
  AcceptInvitationRequest,
  CreateInvitationRequest,
  InvitationAcceptanceDetailsDto,
  InvitationListItemDto
} from '@features/invitations/models/invitation.model';
import { GridQuery, GridResult } from '@query-grid/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InvitationsService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'invitations';

  getInvitations(grid: GridQuery): Observable<GridResult<InvitationListItemDto>> {
    return this.apiService.get<GridResult<InvitationListItemDto>>(this.baseEndpoint, {
      grid
    });
  }

  getInvitationByToken(token: string): Observable<InvitationAcceptanceDetailsDto> {
    return this.apiService.get<InvitationAcceptanceDetailsDto>(
      `${this.baseEndpoint}/accept/${encodeURIComponent(token)}`
    );
  }

  createInvitation(request: CreateInvitationRequest): Observable<{ id: string }> {
    return this.apiService.post<{ id: string }>(this.baseEndpoint, request);
  }

  resendInvitation(id: string): Observable<string> {
    return this.apiService.post<string>(`${this.baseEndpoint}/${id}/resend`, {});
  }

  revokeInvitation(id: string): Observable<string> {
    return this.apiService.post<string>(`${this.baseEndpoint}/${id}/revoke`, {});
  }

  acceptInvitation(
    token: string,
    request: AcceptInvitationRequest
  ): Observable<string> {
    return this.apiService.post<string>(
      `${this.baseEndpoint}/accept/${encodeURIComponent(token)}`,
      request
    );
  }
}

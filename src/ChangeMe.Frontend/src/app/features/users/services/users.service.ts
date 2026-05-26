import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  AdminUserSessionDto,
  EffectivePermissionDto,
  InviteUserRequest,
  PreviewEffectivePermissionsRequest,
  RoleAssignmentOptionDto,
  UpdateUserRequest,
  UserDetailsDto,
  UserListItemDto,
  UserSearchParameters
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'users';

  getUsers(
    params: UserSearchParameters
  ): Observable<PaginationResult<UserListItemDto>> {
    return this.apiService.getPaginated<UserListItemDto, UserSearchParameters>(
      this.baseEndpoint,
      params
    );
  }

  getUserById(id: string): Observable<UserDetailsDto> {
    return this.apiService.get<UserDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getRolesForAssignment(): Observable<RoleAssignmentOptionDto[]> {
    return this.apiService.get<RoleAssignmentOptionDto[]>(`${this.baseEndpoint}/roles`);
  }

  previewEffectivePermissions(
    request: PreviewEffectivePermissionsRequest
  ): Observable<EffectivePermissionDto[]> {
    return this.apiService.get<EffectivePermissionDto[]>(
      `${this.baseEndpoint}/effective-permissions/preview`,
      request
    );
  }

  inviteUser(request: InviteUserRequest): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(this.baseEndpoint, request);
  }

  updateUser(request: UpdateUserRequest): Observable<UserDetailsDto> {
    return this.apiService.put<UserDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  deactivateUser(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/deactivate`,
      {}
    );
  }

  activateUser(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/activate`,
      {}
    );
  }

  resendInvitation(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/resend-invitation`,
      {}
    );
  }

  cancelInvitation(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/cancel-invitation`,
      {}
    );
  }

  sendInvitation(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/send-invitation`,
      {}
    );
  }

  sendPasswordReset(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/send-password-reset`,
      {}
    );
  }

  resetTwoFactor(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/reset-two-factor`,
      {}
    );
  }

  resetPasskeys(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/reset-passkeys`,
      {}
    );
  }

  removePasskey(userId: string, passkeyId: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${userId}/passkeys/${passkeyId}/remove`,
      {}
    );
  }

  unlinkExternalLogin(userId: string, providerKey: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${userId}/external-logins/${encodeURIComponent(providerKey)}/unlink`,
      {}
    );
  }

  confirmUserEmail(id: string): Observable<UserDetailsDto> {
    return this.apiService.post<UserDetailsDto>(
      `${this.baseEndpoint}/${id}/confirm-email`,
      {}
    );
  }

  getUserSessions(
    id: string,
    params: {
      pageNumber: number;
      pageSize: number;
      sortField?: string;
      ascending?: boolean;
    }
  ): Observable<PaginationResult<AdminUserSessionDto>> {
    return this.apiService.getPaginated<AdminUserSessionDto, typeof params>(
      `${this.baseEndpoint}/${id}/sessions`,
      params
    );
  }

  revokeUserSession(userId: string, sessionId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${userId}/sessions/${sessionId}`
    );
  }

  revokeAllUserSessions(userId: string): Observable<boolean> {
    return this.apiService.post<boolean>(
      `${this.baseEndpoint}/${userId}/sessions/revoke-all`,
      {}
    );
  }
}

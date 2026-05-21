import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
    CreateRoleRequest,
    PermissionCatalogItemDto,
    RoleAssignedUserDto,
    RoleAssignedUsersSearchParameters,
    RoleDetailsDto,
    RoleListItemDto,
    RoleSearchParameters,
    UpdateRoleRequest
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RolesService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'roles';

  getRoles(params: RoleSearchParameters): Observable<PaginationResult<RoleListItemDto>> {
    return this.apiService.getPaginated<RoleListItemDto, RoleSearchParameters>(
      this.baseEndpoint,
      params
    );
  }

  getRoleById(id: string): Observable<RoleDetailsDto> {
    return this.apiService.get<RoleDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getPermissionCatalog(): Observable<PermissionCatalogItemDto[]> {
    return this.apiService.get<PermissionCatalogItemDto[]>(
      `${this.baseEndpoint}/permission-catalog`
    );
  }

  createRole(request: CreateRoleRequest): Observable<RoleDetailsDto> {
    return this.apiService.post<RoleDetailsDto>(this.baseEndpoint, request);
  }

  updateRole(request: UpdateRoleRequest): Observable<RoleDetailsDto> {
    return this.apiService.put<RoleDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  deleteRole(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }

  getRoleAssignedUsers(
    roleId: string,
    params: RoleAssignedUsersSearchParameters
  ): Observable<PaginationResult<RoleAssignedUserDto>> {
    return this.apiService.getPaginated<RoleAssignedUserDto, RoleAssignedUsersSearchParameters>(
      `${this.baseEndpoint}/${roleId}/users`,
      params
    );
  }

  removeUserFromRole(roleId: string, userId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${roleId}/users/${userId}`
    );
  }
}

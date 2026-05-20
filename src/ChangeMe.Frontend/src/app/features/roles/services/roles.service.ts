import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  CreateRoleRequest,
  ManageRoleUsersFormDto,
  PermissionCatalogItemDto,
  RoleAssignedUserDto,
  RoleDetailsDto,
  RoleFormDto,
  RoleListItemDto,
  RoleSearchParameters,
  UpdateRoleRequest,
  UpdateRoleUsersRequest
} from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RolesService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'roles';

  getRoles(params?: RoleSearchParameters): Observable<RoleListItemDto[]> {
    return this.apiService.get<RoleListItemDto[]>(this.baseEndpoint, params);
  }

  getRoleById(id: string): Observable<RoleDetailsDto> {
    return this.apiService.get<RoleDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getRoleForm(id: string): Observable<RoleFormDto> {
    return this.apiService.get<RoleFormDto>(`${this.baseEndpoint}/${id}/form`);
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
    searchText?: string
  ): Observable<RoleAssignedUserDto[]> {
    return this.apiService.get<RoleAssignedUserDto[]>(
      `${this.baseEndpoint}/${roleId}/users`,
      searchText ? { searchText } : undefined
    );
  }

  removeUserFromRole(roleId: string, userId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${roleId}/users/${userId}`
    );
  }

  getManageRoleUsersForm(id: string): Observable<ManageRoleUsersFormDto> {
    return this.apiService.get<ManageRoleUsersFormDto>(
      `${this.baseEndpoint}/${id}/manage-users/form`
    );
  }

  updateRoleUsers(request: UpdateRoleUsersRequest): Observable<boolean> {
    return this.apiService.put<boolean>(
      `${this.baseEndpoint}/${request.roleId}/users`,
      request
    );
  }
}

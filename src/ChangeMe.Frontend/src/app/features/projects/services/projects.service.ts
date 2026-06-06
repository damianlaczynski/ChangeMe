import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  AddProjectMemberRequest,
  ChangeProjectMemberRoleRequest,
  CreateProjectRequest,
  ProjectDetailsDto,
  ProjectHistorySearchParameters,
  ProjectListItemDto,
  ProjectMemberDto,
  ProjectMembershipHistoryEntryDto,
  ProjectMembersSearchParameters,
  ProjectOperationHistoryEntryDto,
  ProjectOptionDto,
  ProjectSearchParameters,
  UpdateProjectRequest
} from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectsService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'projects';

  getProjects(
    params: ProjectSearchParameters
  ): Observable<PaginationResult<ProjectListItemDto>> {
    return this.apiService.getPaginated<ProjectListItemDto, ProjectSearchParameters>(
      this.baseEndpoint,
      params
    );
  }

  getProjectById(id: string): Observable<ProjectDetailsDto> {
    return this.apiService.get<ProjectDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getManageableProjects(permissionCode?: string): Observable<ProjectOptionDto[]> {
    return this.apiService.get<ProjectOptionDto[]>(`${this.baseEndpoint}/manageable`, {
      permissionCode
    });
  }

  createProject(request: CreateProjectRequest): Observable<ProjectDetailsDto> {
    return this.apiService.post<ProjectDetailsDto>(this.baseEndpoint, request);
  }

  updateProject(request: UpdateProjectRequest): Observable<ProjectDetailsDto> {
    return this.apiService.put<ProjectDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  deleteProject(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }

  getProjectMembers(
    projectId: string,
    params: ProjectMembersSearchParameters
  ): Observable<PaginationResult<ProjectMemberDto>> {
    return this.apiService.getPaginated<
      ProjectMemberDto,
      ProjectMembersSearchParameters
    >(`${this.baseEndpoint}/${projectId}/members`, params);
  }

  addProjectMember(
    projectId: string,
    request: AddProjectMemberRequest
  ): Observable<boolean> {
    return this.apiService.post<boolean>(
      `${this.baseEndpoint}/${projectId}/members`,
      request
    );
  }

  changeProjectMemberRole(
    projectId: string,
    userId: string,
    request: ChangeProjectMemberRoleRequest
  ): Observable<boolean> {
    return this.apiService.put<boolean>(
      `${this.baseEndpoint}/${projectId}/members/${userId}/role`,
      request
    );
  }

  removeProjectMember(projectId: string, userId: string): Observable<boolean> {
    return this.apiService.delete<boolean>(
      `${this.baseEndpoint}/${projectId}/members/${userId}`
    );
  }

  getProjectMembershipHistory(
    projectId: string,
    params: ProjectHistorySearchParameters
  ): Observable<PaginationResult<ProjectMembershipHistoryEntryDto>> {
    return this.apiService.getPaginated<
      ProjectMembershipHistoryEntryDto,
      ProjectHistorySearchParameters
    >(`${this.baseEndpoint}/${projectId}/membership-history`, params);
  }

  getProjectOperationHistory(
    projectId: string,
    params: ProjectHistorySearchParameters
  ): Observable<PaginationResult<ProjectOperationHistoryEntryDto>> {
    return this.apiService.getPaginated<
      ProjectOperationHistoryEntryDto,
      ProjectHistorySearchParameters
    >(`${this.baseEndpoint}/${projectId}/operation-history`, params);
  }
}

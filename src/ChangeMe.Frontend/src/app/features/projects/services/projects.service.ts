import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  AddProjectMemberRequest,
  CreateProjectRequest,
  ProjectDetailsDto,
  ProjectListItemDto,
  ProjectOverviewDto,
  ProjectSearchParameters,
  ProjectSelectionItemDto,
  UpdateProjectMemberRoleRequest,
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

  getProjectOverview(id: string): Observable<ProjectOverviewDto> {
    return this.apiService.get<ProjectOverviewDto>(
      `${this.baseEndpoint}/${id}/overview`
    );
  }

  getProjectsForSelection(): Observable<ProjectSelectionItemDto[]> {
    return this.apiService.get<ProjectSelectionItemDto[]>(
      `${this.baseEndpoint}/for-selection`
    );
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

  addProjectMember(
    projectId: string,
    request: AddProjectMemberRequest
  ): Observable<ProjectDetailsDto> {
    return this.apiService.post<ProjectDetailsDto>(
      `${this.baseEndpoint}/${projectId}/members`,
      request
    );
  }

  removeProjectMember(
    projectId: string,
    userId: string
  ): Observable<ProjectDetailsDto> {
    return this.apiService.delete<ProjectDetailsDto>(
      `${this.baseEndpoint}/${projectId}/members/${userId}`
    );
  }

  updateProjectMemberRole(
    projectId: string,
    userId: string,
    request: UpdateProjectMemberRoleRequest
  ): Observable<ProjectDetailsDto> {
    return this.apiService.put<ProjectDetailsDto>(
      `${this.baseEndpoint}/${projectId}/members/${userId}`,
      request
    );
  }
}

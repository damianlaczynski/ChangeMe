import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@shared/api/services/api.service';
import {
  AddIssueCommentRequest,
  IssueDto,
  CreateIssueRequest,
  UpdateIssueRequest,
  IssueSearchParameters,
  IssueDetailsDto,
  IssueAssignableUserDto,
  IssueWatchStateDto
} from '../models/issue.model';
import { PaginationResult } from '@shared/data/models/pagination-result.model';

@Injectable({
  providedIn: 'root'
})
export class IssuesService {
  private readonly apiService = inject(ApiService);

  private readonly baseEndpoint = 'issues';

  getAllIssues(params: IssueSearchParameters): Observable<PaginationResult<IssueDto>> {
    return this.apiService.getPaginated<IssueDto, IssueSearchParameters>(
      this.baseEndpoint,
      params
    );
  }

  getIssue(id: string): Observable<IssueDetailsDto> {
    return this.apiService.get<IssueDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getAssignableUsers(): Observable<IssueAssignableUserDto[]> {
    return this.apiService.get<IssueAssignableUserDto[]>(
      `${this.baseEndpoint}/assignable-users`
    );
  }

  createIssue(request: CreateIssueRequest): Observable<IssueDetailsDto> {
    return this.apiService.post<IssueDetailsDto>(this.baseEndpoint, request);
  }

  updateIssue(request: UpdateIssueRequest): Observable<IssueDetailsDto> {
    return this.apiService.put<IssueDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  addComment(
    issueId: string,
    request: AddIssueCommentRequest
  ): Observable<IssueDetailsDto> {
    return this.apiService.post<IssueDetailsDto>(
      `${this.baseEndpoint}/${issueId}/comments`,
      request
    );
  }

  watchIssue(issueId: string): Observable<IssueWatchStateDto> {
    return this.apiService.post<IssueWatchStateDto>(
      `${this.baseEndpoint}/${issueId}/watch`,
      {}
    );
  }

  unwatchIssue(issueId: string): Observable<IssueWatchStateDto> {
    return this.apiService.delete<IssueWatchStateDto>(
      `${this.baseEndpoint}/${issueId}/watch`
    );
  }

  deleteIssue(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }
}

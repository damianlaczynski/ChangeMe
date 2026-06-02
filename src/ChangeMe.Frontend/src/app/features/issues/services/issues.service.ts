import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  AddIssueCommentRequest,
  CreateIssueRequest,
  IssueAssignableUserDto,
  IssueAttachmentDto,
  IssueAttachmentsSearchParameters,
  IssueCommentDto,
  IssueCommentsSearchParameters,
  IssueDetailsDto,
  IssueDto,
  IssueHistoryEntryDto,
  IssueHistorySearchParameters,
  IssueSearchParameters,
  IssueWatchStateDto,
  UpdateIssueRequest
} from '../models/issue.model';

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

  getIssueComments(
    issueId: string,
    params: IssueCommentsSearchParameters
  ): Observable<PaginationResult<IssueCommentDto>> {
    return this.apiService.getPaginated<IssueCommentDto, IssueCommentsSearchParameters>(
      `${this.baseEndpoint}/${issueId}/comments`,
      params
    );
  }

  getIssueHistory(
    issueId: string,
    params: IssueHistorySearchParameters
  ): Observable<PaginationResult<IssueHistoryEntryDto>> {
    return this.apiService.getPaginated<
      IssueHistoryEntryDto,
      IssueHistorySearchParameters
    >(`${this.baseEndpoint}/${issueId}/history`, params);
  }

  getIssueAttachments(
    issueId: string,
    params: IssueAttachmentsSearchParameters
  ): Observable<PaginationResult<IssueAttachmentDto>> {
    return this.apiService.getPaginated<
      IssueAttachmentDto,
      IssueAttachmentsSearchParameters
    >(`${this.baseEndpoint}/${issueId}/attachments`, params);
  }

  uploadIssueAttachment(issueId: string, file: File): Observable<IssueAttachmentDto> {
    const formData = new FormData();
    formData.append('File', file, file.name);

    return this.apiService.postFormData<IssueAttachmentDto>(
      `${this.baseEndpoint}/${issueId}/attachments`,
      formData
    );
  }

  downloadIssueAttachment(issueId: string, attachmentId: string): Observable<Blob> {
    return this.apiService.getBlob(
      `${this.baseEndpoint}/${issueId}/attachments/${attachmentId}/content`
    );
  }

  deleteIssueAttachment(issueId: string, attachmentId: string): Observable<string> {
    return this.apiService.delete<string>(
      `${this.baseEndpoint}/${issueId}/attachments/${attachmentId}`
    );
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

  deleteIssue(id: string): Observable<string> {
    return this.apiService.delete<string>(`${this.baseEndpoint}/${id}`);
  }
}

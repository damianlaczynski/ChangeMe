import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  CreateTimeEntryRequest,
  IssueTimeEntriesResultDto,
  LoggableProjectOptionDto,
  MyTimeEntriesResultDto,
  MyTimeEntriesSearchParameters,
  ReportPersonEntriesSearchParameters,
  ReportPersonEntryDto,
  RunningTimerDto,
  RunningTimerStateDto,
  StartRunningTimerRequest,
  TimeAuditLogSearchParameters,
  TimeEntryAuditLogEntryDto,
  TimeEntryDto,
  TimeReportResultDto,
  TimeReportsSearchParameters,
  TimeSettingsDto,
  UpdateTimeEntryRequest,
  UpdateTimeSettingsRequest
} from '../models/time.model';
import { buildExportReportQuery } from '../utils/time.utils';

@Injectable({
  providedIn: 'root'
})
export class TimeService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'time';

  createTimeEntry(request: CreateTimeEntryRequest): Observable<TimeEntryDto> {
    return this.apiService.post<TimeEntryDto>(`${this.baseEndpoint}/entries`, request);
  }

  updateTimeEntry(
    id: string,
    request: UpdateTimeEntryRequest
  ): Observable<TimeEntryDto> {
    return this.apiService.put<TimeEntryDto>(
      `${this.baseEndpoint}/entries/${id}`,
      request
    );
  }

  deleteTimeEntry(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/entries/${id}`);
  }

  getMyTimeEntries(
    params: MyTimeEntriesSearchParameters
  ): Observable<MyTimeEntriesResultDto> {
    return this.apiService.get<MyTimeEntriesResultDto>(
      `${this.baseEndpoint}/my-entries`,
      params
    );
  }

  getIssueTimeEntries(
    issueId: string,
    params: MyTimeEntriesSearchParameters
  ): Observable<IssueTimeEntriesResultDto> {
    return this.apiService.get<IssueTimeEntriesResultDto>(
      `issues/${issueId}/time-entries`,
      params
    );
  }

  getLoggableProjects(): Observable<LoggableProjectOptionDto[]> {
    return this.apiService.get<LoggableProjectOptionDto[]>(
      `${this.baseEndpoint}/loggable-projects`
    );
  }

  getRunningTimer(): Observable<RunningTimerStateDto> {
    return this.apiService.get<RunningTimerStateDto>(
      `${this.baseEndpoint}/running-timer`
    );
  }

  startRunningTimer(request: StartRunningTimerRequest): Observable<RunningTimerDto> {
    return this.apiService.post<RunningTimerDto>(
      `${this.baseEndpoint}/running-timer`,
      request
    );
  }

  discardRunningTimer(): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/running-timer`);
  }

  getTimeReports(params: TimeReportsSearchParameters): Observable<TimeReportResultDto> {
    return this.apiService.get<TimeReportResultDto>(
      `${this.baseEndpoint}/reports`,
      params
    );
  }

  getReportPersonEntries(
    params: ReportPersonEntriesSearchParameters
  ): Observable<PaginationResult<ReportPersonEntryDto>> {
    return this.apiService.getPaginated<
      ReportPersonEntryDto,
      ReportPersonEntriesSearchParameters
    >(`${this.baseEndpoint}/reports/person-entries`, params);
  }

  exportTimeReports(params: TimeReportsSearchParameters): Observable<Blob> {
    return this.apiService.getBlob(
      `${this.baseEndpoint}/reports/export?${this.buildQueryString(buildExportReportQuery(params))}`
    );
  }

  getTimeAuditLog(
    params: TimeAuditLogSearchParameters
  ): Observable<PaginationResult<TimeEntryAuditLogEntryDto>> {
    return this.apiService.getPaginated<
      TimeEntryAuditLogEntryDto,
      TimeAuditLogSearchParameters
    >(`${this.baseEndpoint}/audit-log`, params);
  }

  getTimeSettings(): Observable<TimeSettingsDto> {
    return this.apiService.get<TimeSettingsDto>(`${this.baseEndpoint}/settings`);
  }

  updateTimeSettings(request: UpdateTimeSettingsRequest): Observable<TimeSettingsDto> {
    return this.apiService.put<TimeSettingsDto>(
      `${this.baseEndpoint}/settings`,
      request
    );
  }

  private buildQueryString(params: Record<string, unknown>): string {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
      if (value === null || value === undefined) {
        return;
      }

      if (Array.isArray(value)) {
        value.forEach((item) => {
          if (item !== null && item !== undefined) {
            searchParams.append(key, String(item));
          }
        });
        return;
      }

      searchParams.set(key, String(value));
    });

    return searchParams.toString();
  }
}

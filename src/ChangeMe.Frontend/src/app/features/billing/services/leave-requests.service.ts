import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  LeaveRequestDetailsDto,
  LeaveRequestListItemDto,
  LeaveRequestSearchParameters,
  MyLeaveRequestSearchParameters,
  RejectLeaveRequestRequest,
  SaveLeaveRequestRequest
} from '../models/leave-request.model';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestsService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'billing/leave-requests';
  private readonly myBaseEndpoint = 'billing/my/leave-requests';

  getLeaveRequests(
    params: LeaveRequestSearchParameters
  ): Observable<PaginationResult<LeaveRequestListItemDto>> {
    return this.apiService.getPaginated<
      LeaveRequestListItemDto,
      LeaveRequestSearchParameters
    >(this.baseEndpoint, params);
  }

  getMyLeaveRequests(
    params: MyLeaveRequestSearchParameters
  ): Observable<PaginationResult<LeaveRequestListItemDto>> {
    return this.apiService.getPaginated<
      LeaveRequestListItemDto,
      MyLeaveRequestSearchParameters
    >(this.myBaseEndpoint, params);
  }

  getLeaveRequestById(id: string): Observable<LeaveRequestDetailsDto> {
    return this.apiService.get<LeaveRequestDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  createLeaveRequest(
    request: SaveLeaveRequestRequest
  ): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(this.baseEndpoint, {
      userId: request.userId,
      leaveTypeId: request.leaveTypeId,
      startDate: request.startDate,
      endDate: request.endDate,
      dayPortion: request.dayPortion,
      reason: request.reason,
      submit: request.submit ?? false
    });
  }

  createMyLeaveRequest(
    request: SaveLeaveRequestRequest
  ): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(this.myBaseEndpoint, {
      leaveTypeId: request.leaveTypeId,
      startDate: request.startDate,
      endDate: request.endDate,
      dayPortion: request.dayPortion,
      reason: request.reason,
      submit: request.submit ?? false
    });
  }

  updateLeaveRequest(
    id: string,
    request: Omit<SaveLeaveRequestRequest, 'userId' | 'submit'>
  ): Observable<LeaveRequestDetailsDto> {
    return this.apiService.put<LeaveRequestDetailsDto>(
      `${this.baseEndpoint}/${id}`,
      request
    );
  }

  submitLeaveRequest(id: string): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(
      `${this.baseEndpoint}/${id}/submit`,
      {}
    );
  }

  approveLeaveRequest(id: string): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(
      `${this.baseEndpoint}/${id}/approve`,
      {}
    );
  }

  rejectLeaveRequest(
    id: string,
    request: RejectLeaveRequestRequest
  ): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(
      `${this.baseEndpoint}/${id}/reject`,
      request
    );
  }

  cancelLeaveRequest(id: string): Observable<LeaveRequestDetailsDto> {
    return this.apiService.post<LeaveRequestDetailsDto>(
      `${this.baseEndpoint}/${id}/cancel`,
      {}
    );
  }

  deleteLeaveRequest(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }
}

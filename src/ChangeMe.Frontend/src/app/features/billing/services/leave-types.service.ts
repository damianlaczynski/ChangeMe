import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  LeaveTypeDetailsDto,
  LeaveTypeListItemDto,
  SaveLeaveTypeRequest
} from '../models/leave-type.model';

@Injectable({
  providedIn: 'root'
})
export class LeaveTypesService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'billing/leave-types';

  getLeaveTypes(): Observable<LeaveTypeListItemDto[]> {
    return this.apiService.get<LeaveTypeListItemDto[]>(this.baseEndpoint);
  }

  getLeaveTypeById(id: string): Observable<LeaveTypeDetailsDto> {
    return this.apiService.get<LeaveTypeDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  createLeaveType(request: SaveLeaveTypeRequest): Observable<LeaveTypeDetailsDto> {
    return this.apiService.post<LeaveTypeDetailsDto>(this.baseEndpoint, request);
  }

  updateLeaveType(
    id: string,
    request: SaveLeaveTypeRequest
  ): Observable<LeaveTypeDetailsDto> {
    return this.apiService.put<LeaveTypeDetailsDto>(
      `${this.baseEndpoint}/${id}`,
      request
    );
  }

  deleteLeaveType(id: string): Observable<boolean> {
    return this.apiService.delete<boolean>(`${this.baseEndpoint}/${id}`);
  }
}

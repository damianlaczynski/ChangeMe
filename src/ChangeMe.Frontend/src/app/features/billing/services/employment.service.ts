import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  EmploymentContractDetailsDto,
  MyEmploymentSummaryDto,
  SaveEmploymentContractRequest,
  UpdateEmploymentContractRequest,
  UpsertEmploymentProfileRequest,
  UserEmploymentDto
} from '../models/employment.model';

@Injectable({
  providedIn: 'root'
})
export class EmploymentService {
  private readonly apiService = inject(ApiService);

  getUserEmployment(userId: string): Observable<UserEmploymentDto> {
    return this.apiService.get<UserEmploymentDto>(`users/${userId}/employment`);
  }

  upsertEmploymentProfile(
    userId: string,
    request: UpsertEmploymentProfileRequest
  ): Observable<UserEmploymentDto['profile']> {
    return this.apiService.put<UserEmploymentDto['profile']>(
      `users/${userId}/employment/profile`,
      request
    );
  }

  getEmploymentContract(
    userId: string,
    contractId: string
  ): Observable<EmploymentContractDetailsDto> {
    return this.apiService.get<EmploymentContractDetailsDto>(
      `users/${userId}/employment/contracts/${contractId}`
    );
  }

  createEmploymentContract(
    userId: string,
    request: SaveEmploymentContractRequest
  ): Observable<EmploymentContractDetailsDto> {
    return this.apiService.post<EmploymentContractDetailsDto>(
      `users/${userId}/employment/contracts`,
      request
    );
  }

  updateEmploymentContract(
    userId: string,
    contractId: string,
    request: UpdateEmploymentContractRequest
  ): Observable<EmploymentContractDetailsDto> {
    return this.apiService.put<EmploymentContractDetailsDto>(
      `users/${userId}/employment/contracts/${contractId}`,
      request
    );
  }

  getMyEmploymentSummary(): Observable<MyEmploymentSummaryDto | null> {
    return this.apiService.get<MyEmploymentSummaryDto | null>('billing/my/employment');
  }
}

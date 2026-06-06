import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  CreateSettlementPeriodRequest,
  MySettlementListItemDto,
  SettlementPeriodDetailsDto,
  SettlementPeriodListItemDto,
  UserSettlementDetailsDto
} from '../models/settlement.model';

@Injectable({
  providedIn: 'root'
})
export class SettlementsService {
  private readonly apiService = inject(ApiService);
  private readonly periodsEndpoint = 'billing/settlement-periods';
  private readonly userSettlementsEndpoint = 'billing/user-settlements';
  private readonly mySettlementsEndpoint = 'billing/my/settlements';

  getSettlementPeriods(): Observable<SettlementPeriodListItemDto[]> {
    return this.apiService.get<SettlementPeriodListItemDto[]>(this.periodsEndpoint);
  }

  getSettlementPeriodById(id: string): Observable<SettlementPeriodDetailsDto> {
    return this.apiService.get<SettlementPeriodDetailsDto>(`${this.periodsEndpoint}/${id}`);
  }

  createSettlementPeriod(
    request: CreateSettlementPeriodRequest
  ): Observable<SettlementPeriodDetailsDto> {
    return this.apiService.post<SettlementPeriodDetailsDto>(this.periodsEndpoint, request);
  }

  recalculateAllSettlements(periodId: string): Observable<SettlementPeriodDetailsDto> {
    return this.apiService.post<SettlementPeriodDetailsDto>(
      `${this.periodsEndpoint}/${periodId}/recalculate-all`,
      {}
    );
  }

  closeSettlementPeriod(periodId: string): Observable<SettlementPeriodDetailsDto> {
    return this.apiService.post<SettlementPeriodDetailsDto>(
      `${this.periodsEndpoint}/${periodId}/close`,
      {}
    );
  }

  getUserSettlementById(id: string): Observable<UserSettlementDetailsDto> {
    return this.apiService.get<UserSettlementDetailsDto>(
      `${this.userSettlementsEndpoint}/${id}`
    );
  }

  recalculateUserSettlement(id: string): Observable<UserSettlementDetailsDto> {
    return this.apiService.post<UserSettlementDetailsDto>(
      `${this.userSettlementsEndpoint}/${id}/recalculate`,
      {}
    );
  }

  getMySettlements(): Observable<MySettlementListItemDto[]> {
    return this.apiService.get<MySettlementListItemDto[]>(this.mySettlementsEndpoint);
  }
}

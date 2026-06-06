import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  BillingSettingsDto,
  LeaveBalanceDto,
  UpdateBillingSettingsRequest
} from '../models/billing-settings.model';

@Injectable({
  providedIn: 'root'
})
export class BillingSettingsService {
  private readonly apiService = inject(ApiService);

  getBillingSettings(): Observable<BillingSettingsDto> {
    return this.apiService.get<BillingSettingsDto>('billing/settings');
  }

  updateBillingSettings(
    request: UpdateBillingSettingsRequest
  ): Observable<BillingSettingsDto> {
    return this.apiService.put<BillingSettingsDto>('billing/settings', request);
  }

  getLeaveBalance(userId: string, year: number): Observable<LeaveBalanceDto> {
    return this.apiService.get<LeaveBalanceDto>('billing/leave-balance', {
      userId,
      year
    });
  }

  getMyLeaveBalance(year: number): Observable<LeaveBalanceDto> {
    return this.apiService.get<LeaveBalanceDto>('billing/my/leave-balance', { year });
  }
}

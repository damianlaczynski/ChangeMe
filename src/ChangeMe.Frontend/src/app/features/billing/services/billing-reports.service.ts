import { Injectable, inject } from '@angular/core';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Observable } from 'rxjs';
import {
  BillingLeaveReportResultDto,
  BillingLeaveReportSearchParameters,
  BillingSettlementReportResultDto,
  BillingSettlementReportSearchParameters,
  SettlementOperationHistorySearchParameters,
  SettlementOperationLogListItemDto
} from '../models/billing-report.model';

@Injectable({
  providedIn: 'root'
})
export class BillingReportsService {
  private readonly apiService = inject(ApiService);

  getSettlementReport(
    params: BillingSettlementReportSearchParameters
  ): Observable<BillingSettlementReportResultDto> {
    return this.apiService.get<BillingSettlementReportResultDto>(
      'billing/reports/settlements',
      params
    );
  }

  getLeaveReport(
    params: BillingLeaveReportSearchParameters
  ): Observable<BillingLeaveReportResultDto> {
    return this.apiService.get<BillingLeaveReportResultDto>(
      'billing/reports/leave',
      params
    );
  }

  getSettlementOperationHistory(
    params: SettlementOperationHistorySearchParameters
  ): Observable<PaginationResult<SettlementOperationLogListItemDto>> {
    return this.apiService.getPaginated<
      SettlementOperationLogListItemDto,
      SettlementOperationHistorySearchParameters
    >('billing/settlement-operation-history', params);
  }
}

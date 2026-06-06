import { ContractType } from '@features/billing/models/employment.model';
import { LeaveRequestStatus } from '@features/billing/models/leave-request.model';

export enum BillingSettlementReportGroupingMode {
  ByPerson = 'ByPerson',
  ByPosition = 'ByPosition',
  ByContractType = 'ByContractType',
  OvertimeSummary = 'OvertimeSummary',
  UndertimeSummary = 'UndertimeSummary'
}

export enum BillingLeaveReportGroupingMode {
  ByPerson = 'ByPerson',
  ByLeaveType = 'ByLeaveType',
  LeaveCalendar = 'LeaveCalendar'
}

export enum SettlementOperationType {
  Created = 'Created',
  Recalculated = 'Recalculated',
  Closed = 'Closed'
}

export interface BillingSettlementReportRowDto {
  label: string;
  userId?: string | null;
  userDisplayName?: string | null;
  positionName?: string | null;
  contractType?: ContractType | null;
  userCount?: number | null;
  expectedMinutes: number;
  loggedMinutes: number;
  leaveDays: number;
  balanceMinutes: number;
}

export interface BillingSettlementReportResultDto {
  groupingMode: BillingSettlementReportGroupingMode;
  periodYear: number;
  periodMonth: number;
  periodLabel: string;
  userCount: number;
  totalExpectedMinutes: number;
  totalLoggedMinutes: number;
  netBalanceMinutes: number;
  rows: BillingSettlementReportRowDto[];
}

export interface BillingLeaveReportRowDto {
  userId?: string | null;
  userDisplayName?: string | null;
  leaveTypeName?: string | null;
  entitledDays?: number | null;
  usedDays?: number | null;
  remainingDays?: number | null;
  totalDays?: number | null;
  requestCount?: number | null;
  datesDisplay?: string | null;
  days?: number | null;
}

export interface BillingLeaveReportResultDto {
  groupingMode: BillingLeaveReportGroupingMode;
  year: number;
  rows: BillingLeaveReportRowDto[];
}

export interface SettlementOperationLogListItemDto {
  id: string;
  timestamp: string;
  actorDisplayName: string;
  operation: SettlementOperationType;
  periodLabel: string;
  userDisplayName?: string | null;
}

export interface SettlementOperationHistorySearchParameters {
  pageNumber: number;
  pageSize: number;
  settlementPeriodId?: string;
}

export interface BillingSettlementReportSearchParameters {
  settlementPeriodId: string;
  userIds?: string[];
  contractTypes?: ContractType[];
  groupingMode: BillingSettlementReportGroupingMode;
}

export interface BillingLeaveReportSearchParameters {
  year: number;
  userIds?: string[];
  leaveTypeIds?: string[];
  statuses?: LeaveRequestStatus[];
  groupingMode: BillingLeaveReportGroupingMode;
}

import { ContractType } from '@features/billing/models/employment.model';

export enum SettlementPeriodStatus {
  Open = 'Open',
  Closed = 'Closed'
}

export interface SettlementPeriodListItemDto {
  id: string;
  year: number;
  month: number;
  label: string;
  status: SettlementPeriodStatus;
}

export interface UserSettlementListItemDto {
  id: string;
  userId: string;
  userDisplayName: string;
  positionName: string;
  contractType?: ContractType | null;
  expectedMinutes: number;
  loggedMinutes: number;
  leaveDays: number;
  balanceMinutes: number;
  lastCalculatedAt: string;
  canRecalculate: boolean;
}

export interface SettlementPeriodDetailsDto {
  id: string;
  year: number;
  month: number;
  label: string;
  status: SettlementPeriodStatus;
  closedAt?: string | null;
  closedByDisplayName?: string | null;
  lastCalculatedAt?: string | null;
  canManage: boolean;
  userSettlements: UserSettlementListItemDto[];
}

export interface UserSettlementContractSummaryDto {
  contractId: string;
  positionName: string;
  contractType: ContractType;
  startDate: string;
  endDate?: string | null;
  fte: number;
  monthlyHoursNormMinutes: number;
}

export interface UserSettlementExpectedTimeDto {
  monthlyHoursNormMinutes: number;
  prorationNote?: string | null;
  paidLeaveMinutesDeducted: number;
  expectedMinutes: number;
}

export interface UserSettlementLeaveItemDto {
  leaveTypeName: string;
  datesDisplay: string;
  days: number;
}

export interface UserSettlementDetailsDto {
  id: string;
  settlementPeriodId: string;
  year: number;
  month: number;
  periodLabel: string;
  userId: string;
  userDisplayName: string;
  contract?: UserSettlementContractSummaryDto | null;
  expectedTime: UserSettlementExpectedTimeDto;
  loggedMinutes: number;
  leaveItems: UserSettlementLeaveItemDto[];
  balanceMinutes: number;
  balanceLabel: string;
  canRecalculate: boolean;
}

export interface MySettlementListItemDto {
  id: string;
  settlementPeriodId: string;
  year: number;
  month: number;
  periodLabel: string;
  expectedMinutes: number;
  loggedMinutes: number;
  leaveDays: number;
  balanceMinutes: number;
}

export interface CreateSettlementPeriodRequest {
  year: number;
  month: number;
}
